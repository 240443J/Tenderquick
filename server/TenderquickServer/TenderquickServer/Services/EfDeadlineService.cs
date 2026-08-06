using Microsoft.EntityFrameworkCore;
using TenderquickServer.Data;
using TenderquickServer.Models;
using TenderquickServer.Models.Deadlines;

namespace TenderquickServer.Services
{
    public class EfDeadlineService : IDeadlineService
    {
        private readonly AppDbContext _db;
        private readonly ICalendarService _calendar;
        private readonly IAuditService _audit;

        public EfDeadlineService(AppDbContext db, ICalendarService calendar, IAuditService audit)
        {
            _db = db;
            _calendar = calendar;
            _audit = audit;
        }

        public async Task<IEnumerable<DeadlineDto>> GetAllAsync(int? tenderId)
        {
            var query = _db.TenderDeadlines.AsNoTracking().Include(d => d.Tender).AsQueryable();

            if (tenderId is not null)
                query = query.Where(d => d.TenderId == tenderId);

            return await query
                .OrderBy(d => d.DueAt)
                .Select(d => new DeadlineDto(
                    d.Id, d.TenderId, d.Tender!.Reference, d.Title, d.Type, d.DueAt,
                    d.AddedToCalendar, d.Priority))
                .ToListAsync();
        }

        public async Task<DeadlineResult> CreateAsync(CreateDeadlineRequest req)
        {
            var type = string.IsNullOrWhiteSpace(req.Type) ? DeadlineType.Closing : req.Type;
            if (!DeadlineType.IsValid(type))
                return new DeadlineResult(DeadlineOutcome.InvalidType, null);

            var tender = await _db.Tenders.FirstOrDefaultAsync(t => t.Id == req.TenderId);
            if (tender is null)
                return new DeadlineResult(DeadlineOutcome.TenderNotFound, null);

            var priority = DeadlinePriority.IsValid(req.Priority)
                ? req.Priority!
                : DerivePriority(req.DueAt);

            var deadline = new TenderDeadline
            {
                TenderId = tender.Id,
                Title = string.IsNullOrWhiteSpace(req.Title) ? $"{tender.Title} — {type}" : req.Title.Trim(),
                Type = type,
                DueAt = req.DueAt,
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _db.TenderDeadlines.Add(deadline);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Deadline.Created", "TenderDeadline", deadline.Id,
                new { tender.Reference, deadline.Type, deadline.DueAt });

            return new DeadlineResult(DeadlineOutcome.Ok, ToDto(deadline, tender.Reference));
        }

        public async Task<DeadlineResult> UpdateAsync(int id, UpdateDeadlineRequest req)
        {
            var deadline = await _db.TenderDeadlines.Include(d => d.Tender).FirstOrDefaultAsync(d => d.Id == id);
            if (deadline is null)
                return new DeadlineResult(DeadlineOutcome.NotFound, null);

            if (req.Type is not null && !DeadlineType.IsValid(req.Type))
                return new DeadlineResult(DeadlineOutcome.InvalidType, null);

            if (!string.IsNullOrWhiteSpace(req.Title)) deadline.Title = req.Title.Trim();
            if (req.Type is not null) deadline.Type = req.Type;
            if (DeadlinePriority.IsValid(req.Priority)) deadline.Priority = req.Priority!;

            if (req.DueAt is not null && req.DueAt != deadline.DueAt)
            {
                deadline.DueAt = req.DueAt.Value;
                // The date moved, so any reminders already sent no longer describe this event.
                deadline.RemindersSent = 0;

                if (deadline.AddedToCalendar)
                {
                    var eventId = await _calendar.PushEventAsync(deadline);
                    deadline.CalendarEventId = eventId;
                    deadline.AddedToCalendar = eventId is not null;
                }
            }

            deadline.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Deadline.Updated", "TenderDeadline", deadline.Id, new { deadline.Type, deadline.DueAt });
            return new DeadlineResult(DeadlineOutcome.Ok, ToDto(deadline, deadline.Tender!.Reference));
        }

        public async Task<DeadlineOutcome> DeleteAsync(int id)
        {
            var deadline = await _db.TenderDeadlines.FirstOrDefaultAsync(d => d.Id == id);
            if (deadline is null) return DeadlineOutcome.NotFound;

            _db.TenderDeadlines.Remove(deadline);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Deadline.Deleted", "TenderDeadline", id, new { deadline.Title });
            return DeadlineOutcome.Ok;
        }

        public async Task<DeadlineResult> AddToCalendarAsync(int id, int userId)
        {
            var status = await _calendar.GetStatusAsync(userId);
            if (!status.Connected)
                return new DeadlineResult(DeadlineOutcome.CalendarNotConnected, null);

            var deadline = await _db.TenderDeadlines.Include(d => d.Tender).FirstOrDefaultAsync(d => d.Id == id);
            if (deadline is null)
                return new DeadlineResult(DeadlineOutcome.NotFound, null);

            var eventId = await _calendar.PushEventAsync(deadline);
            if (eventId is not null)
            {
                deadline.CalendarEventId = eventId;
                deadline.AddedToCalendar = true;
                deadline.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                await _audit.LogAsync("Deadline.CalendarSynced", "TenderDeadline", deadline.Id,
                    new { deadline.Title, eventId });
            }

            return new DeadlineResult(DeadlineOutcome.Ok, ToDto(deadline, deadline.Tender!.Reference));
        }

        public async Task<IEnumerable<DeadlineDto>> SyncAllToCalendarAsync(int userId)
        {
            var status = await _calendar.GetStatusAsync(userId);
            if (!status.Connected)
                return await GetAllAsync(null);

            var pending = await _db.TenderDeadlines
                .Include(d => d.Tender)
                .Where(d => !d.AddedToCalendar)
                .ToListAsync();

            var synced = 0;
            foreach (var deadline in pending)
            {
                var eventId = await _calendar.PushEventAsync(deadline);
                if (eventId is null) continue;

                deadline.CalendarEventId = eventId;
                deadline.AddedToCalendar = true;
                deadline.UpdatedAt = DateTime.UtcNow;
                synced++;
            }

            if (synced > 0)
            {
                await _db.SaveChangesAsync();
                await _audit.LogAsync("Deadline.CalendarSyncedAll", "TenderDeadline", null, new { synced });
            }

            return await GetAllAsync(null);
        }

        private static string DerivePriority(DateTime dueAt)
        {
            var days = (dueAt - DateTime.UtcNow).TotalDays;
            if (days <= 3) return DeadlinePriority.High;
            if (days <= 14) return DeadlinePriority.Medium;
            return DeadlinePriority.Low;
        }

        private static DeadlineDto ToDto(TenderDeadline d, string tenderRef) => new(
            d.Id, d.TenderId, tenderRef, d.Title, d.Type, d.DueAt, d.AddedToCalendar, d.Priority);
    }
}
