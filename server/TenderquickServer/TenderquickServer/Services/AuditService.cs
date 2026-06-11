using System.Security.Claims;
using System.Text.Json;
using TenderquickServer.Data;
using TenderquickServer.Models;

namespace TenderquickServer.Services
{
    public class AuditService : IAuditService
    {
        private readonly InMemoryStore _store;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<AuditService> _logger;

        public AuditService(InMemoryStore store, IHttpContextAccessor http, ILogger<AuditService> logger)
        {
            _store = store;
            _http = http;
            _logger = logger;
        }

        public Task LogAsync(string action, string entityType, int? entityId, object? meta = null)
        {
            var user = _http.HttpContext?.User;
            var userName = user?.FindFirstValue(ClaimTypes.Name) ?? user?.FindFirstValue("name") ?? "System";
            return LogAsAsync(TryGetUserId(user), userName, action, entityType, entityId, meta);
        }

        public Task LogAsAsync(int? userId, string userName, string action, string entityType, int? entityId, object? meta = null)
        {
            // Audit must never break the primary operation — swallow and log on failure.
            try
            {
                var entry = new AuditLog
                {
                    Id = _store.NextAuditId(),
                    UserId = userId,
                    UserName = userName,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    At = DateTime.UtcNow,
                    MetaJson = meta is null ? null : JsonSerializer.Serialize(meta),
                };
                _store.AuditLogs[entry.Id] = entry;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Audit write failed for {Action} {EntityType}", action, entityType);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 50)
        {
            // Id is the stable monotonic key; At can tie within the same millisecond.
            var rows = _store.AuditLogs.Values
                .OrderByDescending(a => a.Id)
                .Take(limit)
                .AsEnumerable();
            return Task.FromResult(rows);
        }

        private static int? TryGetUserId(ClaimsPrincipal? user)
        {
            var sub = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? user?.FindFirstValue("sub");
            return int.TryParse(sub, out var id) ? id : null;
        }
    }
}
