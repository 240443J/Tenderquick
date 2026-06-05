using TenderquickServer.Models;

namespace TenderquickServer.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string? entityType = null, int? entityId = null, object? meta = null);
        Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 50);
    }
}
