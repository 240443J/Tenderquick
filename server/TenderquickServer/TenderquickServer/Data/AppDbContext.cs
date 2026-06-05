using Microsoft.EntityFrameworkCore;
using TenderquickServer.Models;

namespace TenderquickServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Tender> Tenders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Tender>()
                .HasIndex(t => t.Reference)
                .IsUnique();

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.At);
        }
    }
}
