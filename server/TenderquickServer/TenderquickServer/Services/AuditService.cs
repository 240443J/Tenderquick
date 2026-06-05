using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TenderquickServer.Data;
using TenderquickServer.Models;

namespace TenderquickServer.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<AuditService> _logger;

        public AuditService(AppDbContext db, IHttpContextAccessor http, ILogger<AuditService> logger)
        {
            _db = db;
            _http = http;
            _logger = logger;
        }

        public async Task LogAsync(string action, string? entityType = null, int? entityId = null, object? meta = null)
        {
            try
            {
                var principal = _http.HttpContext?.User;
                int? userId = null;
                var idClaim = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idClaim, out var parsed)) userId = parsed;

                var entry = new AuditLog
                {
                    UserId = userId,
                    UserName = principal?.FindFirstValue(ClaimTypes.Name) ?? "system",
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    At = DateTime.UtcNow,
                    MetaJson = meta is null ? null : JsonSerializer.Serialize(meta)
                };

                _db.AuditLogs.Add(entry);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Audit must never break the primary operation.
                _logger.LogWarning(ex, "Failed to write audit log for action {Action}", action);
            }
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 50)
        {
            return await _db.AuditLogs
                .OrderByDescending(a => a.At)
                .Take(Math.Clamp(limit, 1, 200))
                .ToListAsync();
        }
    }
}
