namespace TenderquickServer.Models
{
    public class TenderDocument
    {
        public int Id { get; set; }
        public int TenderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "Technical Proposal";
        public string Status { get; set; } = DocumentStatus.Draft;
        public int Version { get; set; } = 1;
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Tender? Tender { get; set; }
        public ICollection<TenderDocumentSection> Sections { get; set; } = new List<TenderDocumentSection>();
    }

    public class TenderDocumentSection
    {
        public int Id { get; set; }
        public int TenderDocumentId { get; set; }
        public int Ordinal { get; set; }
        public string Heading { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsAiGenerated { get; set; }

        public TenderDocument? TenderDocument { get; set; }
    }

    // Section scaffolding the draft generator fills in. Stored rather than hard-coded so the
    // house style can be edited without a redeploy.
    public class DocumentTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string BodyTemplate { get; set; } = string.Empty;
        public int Ordinal { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class DocumentStatus
    {
        public const string Draft = "Draft";
        public const string InReview = "In Review";
        public const string Final = "Final";

        public static readonly string[] All = { Draft, InReview, Final };

        public static bool IsValid(string? status) => status is not null && All.Contains(status);
    }
}
