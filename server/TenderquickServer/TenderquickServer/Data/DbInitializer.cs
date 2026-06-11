using Microsoft.EntityFrameworkCore;
using TenderquickServer.Models;

namespace TenderquickServer.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            await db.Database.MigrateAsync();

            if (!await db.Users.AnyAsync())
            {
                db.Users.AddRange(
                    NewUser("Admin User", "admin@tenderquick.local", "Admin#123", Roles.Admin),
                    NewUser("Est User", "estimator@tenderquick.local", "Estimator#123", Roles.Estimator),
                    NewUser("View User", "viewer@tenderquick.local", "Viewer#123", Roles.Viewer));
                await db.SaveChangesAsync();
            }

            if (!await db.Tenders.AnyAsync())
            {
                var now = DateTime.UtcNow;
                db.Tenders.AddRange(
                    Tender("NEA/2026/0042", "Cleaning Services for Hawker Centres (Central)", "National Environment Agency",
                        TenderStatus.Interested, 480000m, now.AddDays(12)),
                    Tender("HDB/2026/0117", "LED Lighting Upgrade — Punggol Blocks", "Housing & Development Board",
                        TenderStatus.Drafting, 220000m, now.AddDays(5)),
                    Tender("LTA/2026/0088", "CCTV Maintenance for Bus Interchanges", "Land Transport Authority",
                        TenderStatus.Drafting, 95000m, now.AddDays(2)),
                    Tender("SPORTSG/2025/0203", "AV System Install — Our Tampines Hub", "Sport Singapore",
                        TenderStatus.Won, 310000m, now.AddDays(-40)),
                    Tender("MOE/2025/0451", "School Hall PA System Refresh", "Ministry of Education",
                        TenderStatus.Submitted, 64000m, now.AddDays(-3)),
                    Tender("PUB/2025/0190", "Pump Station Electrical Works", "PUB",
                        TenderStatus.Lost, 150000m, now.AddDays(-60)));
                await db.SaveChangesAsync();
            }
        }

        private static User NewUser(string name, string email, string password, string role) => new()
        {
            Name = name,
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        private static Tender Tender(string reference, string title, string agency,
            string status, decimal estValue, DateTime closingAt)
        {
            var now = DateTime.UtcNow;
            return new Tender
            {
                Reference = reference,
                Title = title,
                Agency = agency,
                Source = TenderSource.Manual,
                Status = status,
                EstValue = estValue,
                ClosingAt = closingAt,
                CreatedAt = now,
                UpdatedAt = now
            };
        }
    }
}
