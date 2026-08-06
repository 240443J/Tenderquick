using TenderquickServer.Models;
using TenderquickServer.Models.Deadlines;

namespace TenderquickServer.Services
{
    // Phase 1 swap point: replace LocalCalendarService with a Google OAuth2 implementation
    // and nothing above this interface changes.
    public interface ICalendarService
    {
        Task<CalendarStatusDto> GetStatusAsync(int userId);
        Task<CalendarStatusDto> ConnectAsync(int userId, string? account);
        Task<CalendarStatusDto> DisconnectAsync(int userId);
        // Returns the external event id, or null when the push failed. Never throws:
        // a calendar outage must not block saving a deadline.
        Task<string?> PushEventAsync(TenderDeadline deadline, CancellationToken ct = default);
    }
}
