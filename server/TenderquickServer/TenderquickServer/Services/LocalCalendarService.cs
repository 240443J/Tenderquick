using Microsoft.EntityFrameworkCore;
using TenderquickServer.Data;
using TenderquickServer.Models;
using TenderquickServer.Models.Deadlines;

namespace TenderquickServer.Services
{
    // Records the connection against the user and mints a local event id. Real Google
    // Calendar sync arrives in Phase 1; this keeps the whole deadline flow exercisable
    // (and the DB shape correct) without OAuth credentials.
    public class LocalCalendarService : ICalendarService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<LocalCalendarService> _logger;

        public LocalCalendarService(AppDbContext db, ILogger<LocalCalendarService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<CalendarStatusDto> GetStatusAsync(int userId)
        {
            var conn = await _db.CalendarConnections
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == "Google");

            return new CalendarStatusDto(conn?.Connected ?? false, conn?.Account);
        }

        public async Task<CalendarStatusDto> ConnectAsync(int userId, string? account)
        {
            var conn = await _db.CalendarConnections
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == "Google");

            if (conn is null)
            {
                conn = new CalendarConnection { UserId = userId, Provider = "Google" };
                _db.CalendarConnections.Add(conn);
            }

            conn.Connected = true;
            conn.Account = account;
            conn.ConnectedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return new CalendarStatusDto(true, conn.Account);
        }

        public async Task<CalendarStatusDto> DisconnectAsync(int userId)
        {
            var conn = await _db.CalendarConnections
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == "Google");

            if (conn is not null)
            {
                conn.Connected = false;
                conn.Account = null;
                conn.ConnectedAt = null;
                await _db.SaveChangesAsync();
            }

            return new CalendarStatusDto(false, null);
        }

        public Task<string?> PushEventAsync(TenderDeadline deadline, CancellationToken ct = default)
        {
            try
            {
                var eventId = $"local-{deadline.Id}-{deadline.DueAt:yyyyMMddHHmm}";
                return Task.FromResult<string?>(eventId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Calendar push failed for deadline {DeadlineId}", deadline.Id);
                return Task.FromResult<string?>(null);
            }
        }
    }
}
