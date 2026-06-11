using Microsoft.EntityFrameworkCore;
using TenderquickServer.Models;

namespace TenderquickServer.Data
{
    // Authored now so entities have one declared schema home. NOT wired to a provider until
    // MySQL is linked — Phase 0 runs entirely on InMemoryStore.
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Tender> Tenders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
