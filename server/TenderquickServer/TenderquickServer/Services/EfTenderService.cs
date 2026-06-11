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
            IQueryable<Tender> query = _db.Tenders;

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

        public async Task<Tender?> GetByIdAsync(int id)
            => await _db.Tenders.FirstOrDefaultAsync(t => t.Id == id);

        public async Task<CreateTenderResult> CreateAsync(CreateTenderRequest req)
        {
            var reference = req.Reference.Trim();
            if (await _db.Tenders.AnyAsync(t => t.Reference == reference))
                return new CreateTenderResult(CreateOutcome.DuplicateReference, null);

            var now = DateTime.UtcNow;
            var tender = new Tender
            {
                Reference = reference,
                Title = req.Title.Trim(),
                Agency = req.Agency.Trim(),
                Source = req.Source ?? "Manual",
                Status = "Interested",
                EstValue = req.EstValue,
                ClosingAt = req.ClosingAt,
                Notes = req.Notes?.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Tenders.Add(tender);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Tender.Created", "Tender", tender.Id, new { tender.Reference });
            return new CreateTenderResult(CreateOutcome.Created, tender);
        }

        public async Task<UpdateTenderResult> UpdateAsync(int id, UpdateTenderRequest req)
        {
            var tender = await _db.Tenders.FirstOrDefaultAsync(t => t.Id == id);
            if (tender is null) return new UpdateTenderResult(UpdateOutcome.NotFound, null);

            tender.Title = req.Title.Trim();
            tender.Agency = req.Agency.Trim();
            tender.Status = req.Status;
            tender.EstValue = req.EstValue;
            tender.ClosingAt = req.ClosingAt;
            tender.Notes = req.Notes?.Trim();
            tender.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _audit.LogAsync("Tender.Updated", "Tender", tender.Id, new { tender.Status });
            return new UpdateTenderResult(UpdateOutcome.Updated, tender);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tender = await _db.Tenders.FirstOrDefaultAsync(t => t.Id == id);
            if (tender is null) return false;

            _db.Tenders.Remove(tender);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Tender.Deleted", "Tender", id, new { tender.Reference });
            return true;
        }
    }
}
