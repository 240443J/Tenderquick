using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TenderquickServer.Data;
using TenderquickServer.Models;

namespace TenderquickServer.Services
{
    public class AuditService : IAuditService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<AuditService> _logger;

        public AuditService(IServiceScopeFactory scopes, IHttpContextAccessor http, ILogger<AuditService> logger)
        {
            _scopes = scopes;
            _http = http;
            _logger = logger;
        }

        public Task LogAsync(string action, string entityType, int? entityId, object? meta = null)
        {
            var user = _http.HttpContext?.User;
            var userName = user?.FindFirstValue(ClaimTypes.Name) ?? user?.FindFirstValue("name") ?? "System";
            return LogAsAsync(TryGetUserId(user), userName, action, entityType, entityId, meta);
        }

        public async Task LogAsAsync(int? userId, string userName, string action, string entityType, int? entityId, object? meta = null)
        {
            // Written through its own scope/DbContext: a failed audit insert must never poison
            // the change tracker of the operation that triggered it.
            try
            {
                using var scope = _scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var json = meta is null ? null : JsonSerializer.Serialize(meta);
                if (json is { Length: > 2000 }) json = json[..2000];

                db.AuditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    UserName = string.IsNullOrWhiteSpace(userName) ? "System" : userName,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    At = DateTime.UtcNow,
                    MetaJson = json,
                });

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Audit write failed for {Action} {EntityType}", action, entityType);
            }
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 50)
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await db.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.Id)
                .Take(limit)
                .ToListAsync();
        }

        private static int? TryGetUserId(ClaimsPrincipal? user)
        {
            var sub = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? user?.FindFirstValue("sub");
            return int.TryParse(sub, out var id) ? id : null;
        }
    }
}
