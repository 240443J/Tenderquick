namespace TenderquickServer.Models
{
    // A candidate found by a portal scan. Persisted (rather than returned and forgotten) so
    // "import" is a stable id the client can post back, and so repeat scans dedupe.
    public class DiscoveredTender
    {
        public int Id { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Agency { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? EstValueRange { get; set; }
        public decimal? EstValue { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? ClosingAt { get; set; }
        public string? DetailUrl { get; set; }
        public int Relevance { get; set; }
        // Comma-separated keyword hits; a join table would be overkill for display-only data.
        public string? MatchedKeywords { get; set; }
        public bool Imported { get; set; }
        public int? ImportedTenderId { get; set; }
        public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    }

    public class KeywordWatch
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Keywords { get; set; } = string.Empty;
        public string Sources { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? LastRunAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
