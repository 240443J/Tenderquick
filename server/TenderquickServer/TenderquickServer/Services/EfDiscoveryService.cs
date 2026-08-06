using Microsoft.EntityFrameworkCore;
using TenderquickServer.Data;
using TenderquickServer.Models;
using TenderquickServer.Models.Deadlines;
using TenderquickServer.Models.Discovery;
using TenderquickServer.Models.Search;
using TenderquickServer.Models.Tenders;
using TenderquickServer.Services.Sources;

namespace TenderquickServer.Services
{
    public class EfDiscoveryService : IDiscoveryService
    {
        private readonly AppDbContext _db;
        private readonly IEnumerable<ITenderSource> _sources;
        private readonly ITenderService _tenders;
        private readonly IDeadlineService _deadlines;
        private readonly IAuditService _audit;
        private readonly ILogger<EfDiscoveryService> _logger;

        public EfDiscoveryService(
            AppDbContext db,
            IEnumerable<ITenderSource> sources,
            ITenderService tenders,
            IDeadlineService deadlines,
            IAuditService audit,
            ILogger<EfDiscoveryService> logger)
        {
            _db = db;
            _sources = sources;
            _tenders = tenders;
            _deadlines = deadlines;
            _audit = audit;
            _logger = logger;
        }

        public IEnumerable<ScrapeSourceDto> GetSources() =>
            _sources.Select(s => new ScrapeSourceDto(
                s.Key,
                s.Name,
                s.IsImplemented,
                s.IsImplemented ? "connected" : "beta",
                SourceNote(s.Key)));

        public async Task<IEnumerable<ScrapeResultDto>> ScanAsync(ScanRequest req, CancellationToken ct = default)
        {
            var keyword = (req.Keyword ?? string.Empty).Trim();
            var limit = Math.Clamp(req.Limit ?? 25, 1, 100);

            var selected = _sources
                .Where(s => req.Sources is null || req.Sources.Count == 0 ||
                            req.Sources.Contains(s.Key, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var terms = keyword
                .Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant())
                .Distinct()
                .ToList();

            var tasks = selected.Select(async source =>
            {
                try
                {
                    return await source.SearchAsync(keyword, limit, ct);
                }
                catch (Exception ex)
                {
                    // One failing portal must not sink the whole scan.
                    _logger.LogWarning(ex, "Scan failed for source {Source}", source.Name);
                    return (IReadOnlyList<ExternalTenderResult>)Array.Empty<ExternalTenderResult>();
                }
            });

            var hits = (await Task.WhenAll(tasks)).SelectMany(x => x).ToList();

            foreach (var hit in hits)
            {
                if (string.IsNullOrWhiteSpace(hit.Reference)) continue;

                var matched = terms
                    .Where(t => $"{hit.Title} {hit.Agency} {hit.Status}".Contains(t, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var relevance = Relevance(terms.Count, matched.Count);

                var existing = await _db.DiscoveredTenders
                    .FirstOrDefaultAsync(d => d.Source == hit.Source && d.Reference == hit.Reference, ct);

                if (existing is null)
                {
                    _db.DiscoveredTenders.Add(new DiscoveredTender
                    {
                        Source = hit.Source,
                        Reference = hit.Reference,
                        Title = Truncate(hit.Title, 300),
                        Agency = Truncate(hit.Agency, 200),
                        Summary = Truncate(hit.Status, 1000),
                        EstValue = hit.Value,
                        EstValueRange = FormatValue(hit.Value),
                        ClosingAt = hit.Date,
                        DetailUrl = Truncate(hit.DetailUrl, 500),
                        Relevance = relevance,
                        MatchedKeywords = Truncate(string.Join(",", matched), 400),
                        DiscoveredAt = DateTime.UtcNow,
                        LastSeenAt = DateTime.UtcNow,
                    });
                }
                else
                {
                    existing.Title = Truncate(hit.Title, 300);
                    existing.Agency = Truncate(hit.Agency, 200);
                    existing.ClosingAt = hit.Date ?? existing.ClosingAt;
                    existing.Relevance = relevance;
                    existing.MatchedKeywords = Truncate(string.Join(",", matched), 400);
                    existing.LastSeenAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync(ct);

            var sourceKeys = selected.Select(s => s.Name).ToList();
            var rows = await _db.DiscoveredTenders
                .AsNoTracking()
                .Where(d => sourceKeys.Contains(d.Source))
                .OrderByDescending(d => d.Relevance)
                .ThenByDescending(d => d.LastSeenAt)
                .Take(limit)
                .ToListAsync(ct);

            await _audit.LogAsync("Discovery.Scanned", "DiscoveredTender", null,
                new { keyword, Sources = sourceKeys, Results = rows.Count });

            return rows.Select(ToDto).ToList();
        }

        public async Task<ImportDiscoveredResponse> ImportAsync(int discoveredId)
        {
            var found = await _db.DiscoveredTenders.FirstOrDefaultAsync(d => d.Id == discoveredId);
            if (found is null)
                return new ImportDiscoveredResponse(false, "Result not found.", null);

            if (found.Imported)
                return new ImportDiscoveredResponse(false, "Already imported.", null);

            var create = new CreateTenderRequest(
                Reference: found.Reference,
                Title: found.Title,
                Agency: found.Agency,
                Source: found.Source,
                EstValue: found.EstValue,
                ClosingAt: found.ClosingAt,
                Notes: found.Summary,
                Specs: null);

            var result = await _tenders.CreateAsync(create);
            if (result.Outcome == CreateOutcome.DuplicateReference)
            {
                found.Imported = true;
                await _db.SaveChangesAsync();
                return new ImportDiscoveredResponse(false, "A tender with this reference already exists.", null);
            }

            var tender = result.Tender!;

            // A discovered tender is worthless without its closing date on the calendar,
            // so the deadline is created as part of the same import.
            if (found.ClosingAt is not null)
            {
                await _deadlines.CreateAsync(new CreateDeadlineRequest(
                    TenderId: tender.Id,
                    Title: $"{tender.Title} — Tender Closing",
                    Type: DeadlineType.Closing,
                    DueAt: found.ClosingAt.Value,
                    Priority: null));
            }

            found.Imported = true;
            found.ImportedTenderId = tender.Id;
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Discovery.Imported", "Tender", tender.Id,
                new { found.Reference, found.Source });

            return new ImportDiscoveredResponse(true, null, tender);
        }

        public async Task<IEnumerable<KeywordWatchDto>> GetWatchesAsync(int userId) =>
            await _db.KeywordWatches
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.Id)
                .Select(w => new KeywordWatchDto(w.Id, w.Keywords, w.Sources, w.IsActive, w.LastRunAt))
                .ToListAsync();

        public async Task<KeywordWatchDto> CreateWatchAsync(int userId, CreateKeywordWatchRequest req)
        {
            var watch = new KeywordWatch
            {
                UserId = userId,
                Keywords = Truncate(req.Keywords, 400) ?? string.Empty,
                Sources = Truncate(req.Sources, 200) ?? "gebiz",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            _db.KeywordWatches.Add(watch);
            await _db.SaveChangesAsync();

            return new KeywordWatchDto(watch.Id, watch.Keywords, watch.Sources, watch.IsActive, watch.LastRunAt);
        }

        public async Task<bool> DeleteWatchAsync(int userId, int id)
        {
            var watch = await _db.KeywordWatches.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
            if (watch is null) return false;

            _db.KeywordWatches.Remove(watch);
            await _db.SaveChangesAsync();
            return true;
        }

        private static int Relevance(int termCount, int matchedCount)
        {
            if (termCount == 0) return 70;
            var ratio = (double)matchedCount / termCount;
            return (int)Math.Round(Math.Clamp(55 + (ratio * 44), 10, 99));
        }

        private static string? FormatValue(decimal? value) =>
            value is null or 0 ? null : $"S${value.Value:N0}";

        private static string? Truncate(string? value, int max) =>
            value is null ? null : value.Length <= max ? value : value[..max];

        private static string SourceNote(string key) => key switch
        {
            "gebiz" => "Government Electronic Business (data.gov.sg)",
            "sesami" => "Healthcare & institutional buyers",
            "tenderboard" => "Aggregated public tenders",
            _ => "External tender portal",
        };

        private static ScrapeResultDto ToDto(DiscoveredTender d) => new(
            d.Id,
            d.Reference,
            d.Title,
            d.Agency,
            d.Source,
            d.PublishedAt,
            d.ClosingAt,
            d.EstValueRange ?? "Not stated",
            d.Relevance,
            string.IsNullOrWhiteSpace(d.MatchedKeywords)
                ? Array.Empty<string>()
                : d.MatchedKeywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            d.Summary,
            d.DetailUrl,
            d.Imported);
    }
}
