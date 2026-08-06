using Microsoft.EntityFrameworkCore;
using TenderquickServer.Data;
using TenderquickServer.Models;
using TenderquickServer.Models.Tenders;

namespace TenderquickServer.Services
{
    public class EfTenderService : ITenderService
    {
        private readonly AppDbContext _db;
        private readonly IAuditService _audit;

        public EfTenderService(AppDbContext db, IAuditService audit)
        {
            _db = db;
            _audit = audit;
        }

        public async Task<IEnumerable<TenderListItem>> GetAllAsync(string? status, string? search)
        {
            IQueryable<Tender> query = _db.Tenders.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(t =>
                    t.Reference.Contains(s) || t.Title.Contains(s) || t.Agency.Contains(s));
            }

            return await query
                .OrderByDescending(t => t.Id)
                .Select(t => new TenderListItem(
                    t.Id, t.Reference, t.Title, t.Agency, t.Source, t.Status, t.EstValue, t.ClosingAt))
                .ToListAsync();
        }

        public async Task<TenderDetail?> GetByIdAsync(int id)
        {
            var tender = await _db.Tenders
                .AsNoTracking()
                .Include(t => t.Specs)
                .FirstOrDefaultAsync(t => t.Id == id);

            return tender is null ? null : ToDetail(tender);
        }

        public async Task<CreateTenderResult> CreateAsync(CreateTenderRequest req)
        {
            var reference = (req.Reference ?? string.Empty).Trim();
            if (await _db.Tenders.AnyAsync(t => t.Reference == reference))
                return new CreateTenderResult(CreateOutcome.DuplicateReference, null);

            var now = DateTime.UtcNow;
            var tender = new Tender
            {
                Reference = reference,
                Title = (req.Title ?? string.Empty).Trim(),
                Agency = (req.Agency ?? string.Empty).Trim(),
                Source = string.IsNullOrWhiteSpace(req.Source) ? "Manual" : req.Source.Trim(),
                Status = TenderStatus.Interested,
                EstValue = req.EstValue,
                ClosingAt = req.ClosingAt,
                Notes = req.Notes?.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
            };

            ApplySpecs(tender, req.Specs);

            _db.Tenders.Add(tender);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Tender.Created", "Tender", tender.Id, new { tender.Reference, tender.Source });
            return new CreateTenderResult(CreateOutcome.Created, ToDetail(tender));
        }

        public async Task<UpdateTenderResult> UpdateAsync(int id, UpdateTenderRequest req)
        {
            if (!TenderStatus.IsValid(req.Status))
                return new UpdateTenderResult(UpdateOutcome.InvalidStatus, null);

            var tender = await _db.Tenders
                .Include(t => t.Specs)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tender is null)
                return new UpdateTenderResult(UpdateOutcome.NotFound, null);

            tender.Title = (req.Title ?? string.Empty).Trim();
            tender.Agency = (req.Agency ?? string.Empty).Trim();
            tender.Status = req.Status;
            tender.EstValue = req.EstValue;
            tender.ClosingAt = req.ClosingAt;
            tender.Notes = req.Notes?.Trim();
            tender.UpdatedAt = DateTime.UtcNow;

            // Absent specs means "not editing them"; an empty array means "clear them".
            if (req.Specs is not null)
            {
                _db.TenderSpecs.RemoveRange(tender.Specs);
                tender.Specs.Clear();
                ApplySpecs(tender, req.Specs);
            }

            await _db.SaveChangesAsync();

            await _audit.LogAsync("Tender.Updated", "Tender", tender.Id, new { tender.Reference, tender.Status });
            return new UpdateTenderResult(UpdateOutcome.Updated, ToDetail(tender));
        }

        public async Task<DeleteOutcome> DeleteAsync(int id)
        {
            var tender = await _db.Tenders.FirstOrDefaultAsync(t => t.Id == id);
            if (tender is null) return DeleteOutcome.NotFound;

            if (await _db.Quotations.AnyAsync(q => q.TenderId == id))
                return DeleteOutcome.HasQuotations;

            _db.Tenders.Remove(tender);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Tender.Deleted", "Tender", id, new { tender.Reference });
            return DeleteOutcome.Deleted;
        }

        private static void ApplySpecs(Tender tender, IReadOnlyList<string>? specs)
        {
            if (specs is null) return;

            var ordinal = 0;
            foreach (var text in specs)
            {
                if (string.IsNullOrWhiteSpace(text)) continue;
                tender.Specs.Add(new TenderSpec
                {
                    Ordinal = ordinal++,
                    Text = text.Trim().Length > 1000 ? text.Trim()[..1000] : text.Trim(),
                });
            }
        }

        private static TenderDetail ToDetail(Tender t) => new(
            t.Id, t.Reference, t.Title, t.Agency, t.Source, t.Status, t.EstValue, t.ClosingAt,
            t.Notes, t.DetailUrl, t.CreatedAt, t.UpdatedAt,
            t.Specs.OrderBy(s => s.Ordinal).Select(s => s.Text).ToList());
    }
}
