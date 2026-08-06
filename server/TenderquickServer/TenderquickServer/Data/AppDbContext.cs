using Microsoft.EntityFrameworkCore;
using TenderquickServer.Models;

namespace TenderquickServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Tender> Tenders { get; set; } = null!;
        public DbSet<TenderSpec> TenderSpecs { get; set; } = null!;
        public DbSet<TenderDeadline> TenderDeadlines { get; set; } = null!;
        public DbSet<CalendarConnection> CalendarConnections { get; set; } = null!;
        public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
        public DbSet<PriceHistory> PriceHistories { get; set; } = null!;
        public DbSet<LabourRate> LabourRates { get; set; } = null!;
        public DbSet<LabourRateHistory> LabourRateHistories { get; set; } = null!;
        public DbSet<Quotation> Quotations { get; set; } = null!;
        public DbSet<QuotationLine> QuotationLines { get; set; } = null!;
        public DbSet<QuotationSignoff> QuotationSignoffs { get; set; } = null!;
        public DbSet<TenderDocument> TenderDocuments { get; set; } = null!;
        public DbSet<TenderDocumentSection> TenderDocumentSections { get; set; } = null!;
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; } = null!;
        public DbSet<AiMemory> AiMemories { get; set; } = null!;
        public DbSet<AiPreference> AiPreferences { get; set; } = null!;
        public DbSet<AiInteraction> AiInteractions { get; set; } = null!;
        public DbSet<DiscoveredTender> DiscoveredTenders { get; set; } = null!;
        public DbSet<KeywordWatch> KeywordWatches { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<User>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(120).IsRequired();
                e.Property(x => x.Email).HasMaxLength(160).IsRequired();
                e.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
                e.Property(x => x.Role).HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.Email).IsUnique();
            });

            b.Entity<Tender>(e =>
            {
                e.Property(x => x.Reference).HasMaxLength(120).IsRequired();
                e.Property(x => x.Title).HasMaxLength(300).IsRequired();
                e.Property(x => x.Agency).HasMaxLength(200).IsRequired();
                e.Property(x => x.Source).HasMaxLength(40).IsRequired();
                e.Property(x => x.Status).HasMaxLength(20).IsRequired();
                e.Property(x => x.EstValue).HasPrecision(18, 2);
                e.Property(x => x.Notes).HasMaxLength(4000);
                e.Property(x => x.DetailUrl).HasMaxLength(500);
                e.HasIndex(x => x.Reference).IsUnique();
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.ClosingAt);
            });

            b.Entity<TenderSpec>(e =>
            {
                e.Property(x => x.Text).HasMaxLength(1000).IsRequired();
                e.HasIndex(x => new { x.TenderId, x.Ordinal });
                e.HasOne(x => x.Tender)
                    .WithMany(t => t.Specs)
                    .HasForeignKey(x => x.TenderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<TenderDeadline>(e =>
            {
                e.Property(x => x.Title).HasMaxLength(300).IsRequired();
                e.Property(x => x.Type).HasMaxLength(30).IsRequired();
                e.Property(x => x.Priority).HasMaxLength(10).IsRequired();
                e.Property(x => x.CalendarEventId).HasMaxLength(200);
                e.HasIndex(x => x.TenderId);
                e.HasIndex(x => x.DueAt);
                e.HasOne(x => x.Tender)
                    .WithMany(t => t.Deadlines)
                    .HasForeignKey(x => x.TenderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<CalendarConnection>(e =>
            {
                e.Property(x => x.Provider).HasMaxLength(30).IsRequired();
                e.Property(x => x.Account).HasMaxLength(160);
                e.HasIndex(x => new { x.UserId, x.Provider }).IsUnique();
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<InventoryItem>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.Category).HasMaxLength(60).IsRequired();
                e.Property(x => x.Unit).HasMaxLength(20).IsRequired();
                e.Property(x => x.SupplierName).HasMaxLength(160);
                e.Property(x => x.LastTenderRef).HasMaxLength(120);
                e.HasIndex(x => x.Name);
                e.HasIndex(x => x.Category);
            });

            b.Entity<PriceHistory>(e =>
            {
                e.Property(x => x.UnitCost).HasPrecision(18, 2);
                e.Property(x => x.SourceTenderRef).HasMaxLength(120);
                e.HasIndex(x => new { x.InventoryItemId, x.EffectiveFrom });
                e.HasOne(x => x.InventoryItem)
                    .WithMany(i => i.Prices)
                    .HasForeignKey(x => x.InventoryItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<LabourRate>(e =>
            {
                e.Property(x => x.Role).HasMaxLength(160).IsRequired();
                e.Property(x => x.Unit).HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.Role);
            });

            b.Entity<LabourRateHistory>(e =>
            {
                e.Property(x => x.HourlyRate).HasPrecision(18, 2);
                e.HasIndex(x => new { x.LabourRateId, x.EffectiveFrom });
                e.HasOne(x => x.LabourRate)
                    .WithMany(l => l.Rates)
                    .HasForeignKey(x => x.LabourRateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<Quotation>(e =>
            {
                e.Property(x => x.QuoteNo).HasMaxLength(40).IsRequired();
                e.Property(x => x.Title).HasMaxLength(300).IsRequired();
                e.Property(x => x.Client).HasMaxLength(200).IsRequired();
                e.Property(x => x.Status).HasMaxLength(20).IsRequired();
                e.Property(x => x.VerifiedBy).HasMaxLength(120);
                e.Property(x => x.MarkupPct).HasPrecision(6, 2);
                e.Property(x => x.GstPct).HasPrecision(6, 2);
                e.Property(x => x.Subtotal).HasPrecision(18, 2);
                e.Property(x => x.Total).HasPrecision(18, 2);
                e.HasIndex(x => x.QuoteNo).IsUnique();
                e.HasIndex(x => x.TenderId);
                // Quotations are financial records: deleting a tender must not silently
                // take its priced work with it.
                e.HasOne(x => x.Tender)
                    .WithMany(t => t.Quotations)
                    .HasForeignKey(x => x.TenderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<QuotationLine>(e =>
            {
                e.Property(x => x.Kind).HasMaxLength(20).IsRequired();
                e.Property(x => x.Description).HasMaxLength(500).IsRequired();
                e.Property(x => x.Unit).HasMaxLength(20).IsRequired();
                e.Property(x => x.Qty).HasPrecision(18, 3);
                e.Property(x => x.UnitPrice).HasPrecision(18, 2);
                e.HasIndex(x => new { x.QuotationId, x.Ordinal });
                e.HasOne(x => x.Quotation)
                    .WithMany(q => q.Lines)
                    .HasForeignKey(x => x.QuotationId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.InventoryItem)
                    .WithMany()
                    .HasForeignKey(x => x.InventoryItemId)
                    .OnDelete(DeleteBehavior.SetNull);
                e.HasOne(x => x.LabourRate)
                    .WithMany()
                    .HasForeignKey(x => x.LabourRateId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            b.Entity<QuotationSignoff>(e =>
            {
                e.Property(x => x.UserName).HasMaxLength(120).IsRequired();
                e.HasIndex(x => new { x.QuotationId, x.QuoteVersion });
                e.HasOne(x => x.Quotation)
                    .WithMany(q => q.Signoffs)
                    .HasForeignKey(x => x.QuotationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<TenderDocument>(e =>
            {
                e.Property(x => x.Title).HasMaxLength(300).IsRequired();
                e.Property(x => x.Type).HasMaxLength(60).IsRequired();
                e.Property(x => x.Status).HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.TenderId);
                e.HasOne(x => x.Tender)
                    .WithMany(t => t.Documents)
                    .HasForeignKey(x => x.TenderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<TenderDocumentSection>(e =>
            {
                e.Property(x => x.Heading).HasMaxLength(200).IsRequired();
                e.Property(x => x.Body).HasColumnType("text");
                e.HasIndex(x => new { x.TenderDocumentId, x.Ordinal });
                e.HasOne(x => x.TenderDocument)
                    .WithMany(d => d.Sections)
                    .HasForeignKey(x => x.TenderDocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<DocumentTemplate>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(160).IsRequired();
                e.Property(x => x.Section).HasMaxLength(200).IsRequired();
                e.Property(x => x.BodyTemplate).HasColumnType("text");
                e.HasIndex(x => x.Ordinal);
            });

            b.Entity<AiMemory>(e =>
            {
                e.HasIndex(x => x.UserId).IsUnique();
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<AiPreference>(e =>
            {
                e.Property(x => x.Text).HasMaxLength(500).IsRequired();
                e.Property(x => x.Source).HasMaxLength(300).IsRequired();
                e.Property(x => x.Confidence).HasPrecision(4, 3);
                e.HasIndex(x => x.AiMemoryId);
                e.HasOne(x => x.AiMemory)
                    .WithMany(m => m.Preferences)
                    .HasForeignKey(x => x.AiMemoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<AiInteraction>(e =>
            {
                e.Property(x => x.Feature).HasMaxLength(60).IsRequired();
                e.Property(x => x.Model).HasMaxLength(80).IsRequired();
                e.Property(x => x.EntityType).HasMaxLength(60);
                e.Property(x => x.Outcome).HasMaxLength(40);
                e.Property(x => x.Prompt).HasColumnType("text");
                e.Property(x => x.Response).HasColumnType("mediumtext");
                e.HasIndex(x => new { x.Feature, x.CreatedAt });
                e.HasIndex(x => x.TenderId);
            });

            b.Entity<DiscoveredTender>(e =>
            {
                e.Property(x => x.Source).HasMaxLength(40).IsRequired();
                e.Property(x => x.Reference).HasMaxLength(120).IsRequired();
                e.Property(x => x.Title).HasMaxLength(300).IsRequired();
                e.Property(x => x.Agency).HasMaxLength(200).IsRequired();
                e.Property(x => x.Summary).HasMaxLength(1000);
                e.Property(x => x.EstValueRange).HasMaxLength(80);
                e.Property(x => x.EstValue).HasPrecision(18, 2);
                e.Property(x => x.DetailUrl).HasMaxLength(500);
                e.Property(x => x.MatchedKeywords).HasMaxLength(400);
                e.HasIndex(x => new { x.Source, x.Reference }).IsUnique();
                e.HasIndex(x => x.Relevance);
            });

            b.Entity<KeywordWatch>(e =>
            {
                e.Property(x => x.Keywords).HasMaxLength(400).IsRequired();
                e.Property(x => x.Sources).HasMaxLength(200).IsRequired();
                e.HasIndex(x => x.UserId);
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<AuditLog>(e =>
            {
                e.Property(x => x.UserName).HasMaxLength(120).IsRequired();
                e.Property(x => x.Action).HasMaxLength(80).IsRequired();
                e.Property(x => x.EntityType).HasMaxLength(60);
                e.Property(x => x.MetaJson).HasMaxLength(2000);
                e.HasIndex(x => x.At);
                e.HasIndex(x => new { x.EntityType, x.EntityId });
            });
        }
    }
}
