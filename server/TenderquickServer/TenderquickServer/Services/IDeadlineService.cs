using TenderquickServer.Models.Deadlines;

namespace TenderquickServer.Services
{
    public enum DeadlineOutcome { Ok, NotFound, InvalidType, TenderNotFound, CalendarNotConnected }

    public record DeadlineResult(DeadlineOutcome Outcome, DeadlineDto? Deadline);

    public interface IDeadlineService
    {
        Task<IEnumerable<DeadlineDto>> GetAllAsync(int? tenderId);
        Task<DeadlineResult> CreateAsync(CreateDeadlineRequest req);
        Task<DeadlineResult> UpdateAsync(int id, UpdateDeadlineRequest req);
        Task<DeadlineOutcome> DeleteAsync(int id);
        Task<DeadlineResult> AddToCalendarAsync(int id, int userId);
        Task<IEnumerable<DeadlineDto>> SyncAllToCalendarAsync(int userId);
    }
}
