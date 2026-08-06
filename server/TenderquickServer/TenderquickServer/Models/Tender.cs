namespace TenderquickServer.Models
{
    public class Tender
    {
        public int Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Agency { get; set; } = string.Empty;
        public string Source { get; set; } = "Manual";
        public string Status { get; set; } = "Interested";
        public decimal? EstValue { get; set; }
        public DateTime? ClosingAt { get; set; }
        public string? Notes { get; set; }
        public string? DetailUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TenderSpec> Specs { get; set; } = new List<TenderSpec>();
        public ICollection<TenderDeadline> Deadlines { get; set; } = new List<TenderDeadline>();
        public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
        public ICollection<TenderDocument> Documents { get; set; } = new List<TenderDocument>();
    }

    // One requirement line lifted from the tender specification. Ordered, because the AI
    // drafting and quotation features quote them back to the buyer in the original order.
    public class TenderSpec
    {
        public int Id { get; set; }
        public int TenderId { get; set; }
        public int Ordinal { get; set; }
        public string Text { get; set; } = string.Empty;

        public Tender? Tender { get; set; }
    }
}
