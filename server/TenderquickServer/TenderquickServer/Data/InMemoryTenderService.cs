using TenderquickServer.Models;
using TenderquickServer.Models.Tenders;
using TenderquickServer.Services;

namespace TenderquickServer.Data
{
    public class InMemoryTenderService : ITenderService
    {
        private readonly InMemoryStore _store;
        private readonly IAuditService _audit;

        public InMemoryTenderService(InMemoryStore store, IAuditService audit)
        {
            _store = store;
            _audit = audit;
        }

        public Task<IEnumerable<TenderListItem>> GetAllAsync(string? status, string? search)
        {
            IEnumerable<Tender> query = _store.Tenders.Values;

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => string.Equals(t.Status, status, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(t =>
                    t.Reference.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    t.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    t.Agency.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            // Id is monotonic — stable ordering even when CreatedAt ties (e.g. seeded rows).
            var items = query
                .OrderByDescending(t => t.Id)
                .Select(t => new TenderListItem(t.Id, t.Reference, t.Title, t.Agency, t.Source, t.Status, t.EstValue, t.ClosingAt))
                .AsEnumerable();

            return Task.FromResult(items);
        }

        public Task<Tender?> GetByIdAsync(int id)
        {
            _store.Tenders.TryGetValue(id, out var tender);
            return Task.FromResult(tender);
        }

        public async Task<CreateTenderResult> CreateAsync(CreateTenderRequest req)
        {
            var exists = _store.Tenders.Values.Any(t =>
                string.Equals(t.Reference, req.Reference, StringComparison.OrdinalIgnoreCase));
            if (exists)
                return new CreateTenderResult(CreateOutcome.DuplicateReference, null);

            var tender = new Tender
            {
                Id = _store.NextTenderId(),
                Reference = req.Reference,
                Title = req.Title,
                Agency = req.Agency,
                Source = string.IsNullOrWhiteSpace(req.Source) ? "Manual" : req.Source,
                Status = "Interested",
                EstValue = req.EstValue,
                ClosingAt = req.ClosingAt,
                Notes = req.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _store.Tenders[tender.Id] = tender;
            await _audit.LogAsync("Tender.Created", "Tender", tender.Id, new { tender.Reference, tender.Source });

            return new CreateTenderResult(CreateOutcome.Created, tender);
        }

        public async Task<UpdateTenderResult> UpdateAsync(int id, UpdateTenderRequest req)
        {
            if (!TenderStatus.IsValid(req.Status))
                return new UpdateTenderResult(UpdateOutcome.InvalidStatus, null);

            if (!_store.Tenders.TryGetValue(id, out var tender))
                return new UpdateTenderResult(UpdateOutcome.NotFound, null);

            tender.Title = req.Title;
            tender.Agency = req.Agency;
            tender.Status = req.Status;
            tender.EstValue = req.EstValue;
            tender.ClosingAt = req.ClosingAt;
            tender.Notes = req.Notes;
            tender.UpdatedAt = DateTime.UtcNow;

            await _audit.LogAsync("Tender.Updated", "Tender", tender.Id, new { tender.Reference, tender.Status });
            return new UpdateTenderResult(UpdateOutcome.Updated, tender);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!_store.Tenders.TryRemove(id, out var removed))
                return false;

            await _audit.LogAsync("Tender.Deleted", "Tender", id, new { removed.Reference });
            return true;
        }
    }
}
