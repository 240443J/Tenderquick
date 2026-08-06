namespace TenderquickServer.Models
{
    public class Quotation
    {
        public int Id { get; set; }
        public string QuoteNo { get; set; } = string.Empty;
        public int TenderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;
        public string Status { get; set; } = QuotationStatus.Draft;
        public int Version { get; set; } = 1;
        public decimal MarkupPct { get; set; } = 15m;
        public decimal GstPct { get; set; } = 9m;
        // Denormalised on every save so list views and reports don't re-sum line items.
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public bool Verified { get; set; }
        public string? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Tender? Tender { get; set; }
        public ICollection<QuotationLine> Lines { get; set; } = new List<QuotationLine>();
        public ICollection<QuotationSignoff> Signoffs { get; set; } = new List<QuotationSignoff>();
    }

    public class QuotationLine
    {
        public int Id { get; set; }
        public int QuotationId { get; set; }
        public int Ordinal { get; set; }
        public string Kind { get; set; } = QuotationLineKind.Equipment;
        public string Description { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public string Unit { get; set; } = "each";
        // Snapshot of the price at draft time — inventory repricing must not silently
        // change a quotation that has already been shown to a client.
        public decimal UnitPrice { get; set; }
        public int? InventoryItemId { get; set; }
        public int? LabourRateId { get; set; }
        public bool IsAiSuggested { get; set; }

        public Quotation? Quotation { get; set; }
        public InventoryItem? InventoryItem { get; set; }
        public LabourRate? LabourRate { get; set; }
    }

    // The "a human checked this" record. Immutable and version-scoped: editing a signed
    // quotation bumps the version, which leaves this row behind as history.
    public class QuotationSignoff
    {
        public int Id { get; set; }
        public int QuotationId { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int QuoteVersion { get; set; }
        public DateTime SignedAt { get; set; } = DateTime.UtcNow;

        public Quotation? Quotation { get; set; }
    }

    public static class QuotationStatus
    {
        public const string Draft = "Draft";
        public const string AwaitingSignoff = "AwaitingSignoff";
        public const string Verified = "Verified";

        public static readonly string[] All = { Draft, AwaitingSignoff, Verified };

        public static bool IsValid(string? status) => status is not null && All.Contains(status);
    }

    public static class QuotationLineKind
    {
        public const string Equipment = "equipment";
        public const string Labour = "labour";

        public static readonly string[] All = { Equipment, Labour };

        public static bool IsValid(string? kind) => kind is not null && All.Contains(kind);
    }
}
