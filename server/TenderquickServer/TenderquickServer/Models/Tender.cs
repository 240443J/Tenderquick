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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
