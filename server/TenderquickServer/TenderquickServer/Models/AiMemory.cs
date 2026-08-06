namespace TenderquickServer.Models
{
    // The corpus that makes drafting improve with use. One shared team memory (UserId null)
    // plus optional per-user memories.
    public class AiMemory
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int SamplesLearned { get; set; }
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public ICollection<AiPreference> Preferences { get; set; } = new List<AiPreference>();
    }

    public class AiPreference
    {
        public int Id { get; set; }
        public int AiMemoryId { get; set; }
        public string Text { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public string Source { get; set; } = string.Empty;
        public int TimesApplied { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public AiMemory? AiMemory { get; set; }
    }

    // Every generation call is logged: cost tracking now, retrieval corpus later.
    public class AiInteraction
    {
        public int Id { get; set; }
        public string Feature { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int? TenderId { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? Prompt { get; set; }
        public string? Response { get; set; }
        public int TokensIn { get; set; }
        public int TokensOut { get; set; }
        public int? HumanEditDelta { get; set; }
        public string? Outcome { get; set; }
        public int? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class AiFeature
    {
        public const string QuotationDraft = "QuotationDraft";
        public const string DocumentDraft = "DocumentDraft";
        public const string DocumentEdit = "DocumentEdit";
    }
}
