using System.Collections.Concurrent;
using TenderquickServer.Models;

namespace TenderquickServer.Data
{
    // Singleton mock-first store. Swap target: when MySQL is linked, services move to AppDbContext
    // and this is dropped — controllers depend only on service interfaces, so the swap is local.
    public class InMemoryStore
    {
        public ConcurrentDictionary<int, User> Users { get; } = new();
        public ConcurrentDictionary<int, Tender> Tenders { get; } = new();
        public ConcurrentDictionary<int, AuditLog> AuditLogs { get; } = new();

        private int _userId;
        private int _tenderId;
        private int _auditId;

        public InMemoryStore()
        {
            SeedUsers();
            Seed();
        }

        public int NextUserId() => Interlocked.Increment(ref _userId);
        public int NextTenderId() => Interlocked.Increment(ref _tenderId);
        public int NextAuditId() => Interlocked.Increment(ref _auditId);

        private void SeedUsers()
        {
            // Dev-only credentials (documented in the Phase 0 plan), BCrypt-hashed at startup.
            var seeds = new[]
            {
                (Name: "Admin User", Email: "admin@tenderquick.local", Password: "Admin#123", Role: Roles.Admin),
                (Name: "Est User", Email: "estimator@tenderquick.local", Password: "Estimator#123", Role: Roles.Estimator),
                (Name: "View User", Email: "viewer@tenderquick.local", Password: "Viewer#123", Role: Roles.Viewer),
            };

            foreach (var s in seeds)
            {
                var user = new User
                {
                    Id = NextUserId(),
                    Name = s.Name,
                    Email = s.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(s.Password),
                    Role = s.Role,
                };
                Users[user.Id] = user;
            }
        }

        private void Seed()
        {
            var samples = new[]
            {
                new Tender { Reference = "NEA000ETT24000123", Title = "Cleaning Services for Hawker Centres", Agency = "National Environment Agency", Source = "Manual", Status = "Interested", EstValue = 480000m, ClosingAt = DateTime.UtcNow.AddDays(9) },
                new Tender { Reference = "HDB000ETT24000077", Title = "LED Lighting Upgrade at Common Areas", Agency = "Housing & Development Board", Source = "Manual", Status = "Drafting", EstValue = 1250000m, ClosingAt = DateTime.UtcNow.AddDays(4) },
                new Tender { Reference = "SSG000ETT23000910", Title = "AV Installation for Sports Hub", Agency = "Sport Singapore", Source = "Manual", Status = "Won", EstValue = 360000m },
            };

            foreach (var t in samples)
            {
                t.Id = NextTenderId();
                Tenders[t.Id] = t;
            }
        }
    }
}
