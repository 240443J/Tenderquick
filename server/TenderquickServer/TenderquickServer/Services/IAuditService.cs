using TenderquickServer.Models;

namespace TenderquickServer.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityType, int? entityId, object? meta = null);
        // For flows where the actor isn't in HttpContext claims yet (e.g. login itself).
        Task LogAsAsync(int? userId, string userName, string action, string entityType, int? entityId, object? meta = null);
        Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 50);
    }
}
