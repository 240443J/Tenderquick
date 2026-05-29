using Microsoft.EntityFrameworkCore;

namespace TenderquickServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Add DbSet<T> for each entity here, e.g.:
        // public DbSet<User> Users { get; set; }
    }
}
