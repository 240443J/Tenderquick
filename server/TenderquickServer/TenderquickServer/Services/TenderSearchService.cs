using TenderquickServer.Models.Search;
using TenderquickServer.Models.Tenders;
using TenderquickServer.Services.Sources;

namespace TenderquickServer.Services
{
    public class TenderSearchService : ITenderSearchService
    {
        private readonly IEnumerable<ITenderSource> _sources;
        private readonly ITenderService _tenders;
        private readonly ILogger<TenderSearchService> _logger;

        public TenderSearchService(
            IEnumerable<ITenderSource> sources,
            ITenderService tenders,
            ILogger<TenderSearchService> logger)
        {
            _sources = sources;
            _tenders = tenders;
            _logger = logger;
        }

        public async Task<TenderSearchResponse> SearchAsync(
            string keyword, IReadOnlyList<string>? sourceKeys, int limit, CancellationToken ct)
        {
            var selected = _sources
                .Where(s => sourceKeys is null || sourceKeys.Count == 0 ||
                            sourceKeys.Contains(s.Key, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var tasks = selected.Select(async source =>
            {
                try
                {
                    var hits = await source.SearchAsync(keyword, limit, ct);
                    return (source, hits, ok: true, msg: (string?)null);
                }
                catch (Exception ex)
                {
                    // One bad source must not fail the whole search.
                    _logger.LogWarning(ex, "Source {Source} search failed", source.Name);
                    return (source, (IReadOnlyList<ExternalTenderResult>)Array.Empty<ExternalTenderResult>(),
                            ok: false, msg: (string?)"Source unavailable");
                }
            });

            var perSource = await Task.WhenAll(tasks);

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var merged = new List<ExternalTenderResult>();
            var statuses = new List<SourceStatus>();

            foreach (var (source, hits, ok, msg) in perSource)
            {
                // Sources already keyword-filter server-side; here we only merge + de-dupe.
                var added = 0;
                foreach (var hit in hits)
                {
                    if (seen.Add($"{hit.Source}|{hit.Reference}"))
                    {
                        merged.Add(hit);
                        added++;
                    }
                }
                statuses.Add(new SourceStatus(source.Name, added, ok,
                    msg ?? (!source.IsImplemented ? "Not yet supported" : null)));
            }

            return new TenderSearchResponse(keyword, merged, statuses);
        }

        public async Task<ImportResultsResponse> ImportAsync(IReadOnlyList<ExternalTenderResult> items)
        {
            int imported = 0, skipped = 0;
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Reference)) { skipped++; continue; }

                var req = new CreateTenderRequest(
                    Reference: item.Reference,
                    Title: item.Title,
                    Agency: item.Agency,
                    Source: item.Source,
                    EstValue: item.Value,
                    ClosingAt: item.Date,
                    Notes: null);

                var result = await _tenders.CreateAsync(req);
                if (result.Outcome == CreateOutcome.Created) imported++;
                else skipped++;
            }
            return new ImportResultsResponse(imported, skipped);
        }
    }
}
