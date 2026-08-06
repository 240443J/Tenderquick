namespace TenderquickServer.Models
{
    public class TenderDeadline
    {
        public int Id { get; set; }
        public int TenderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = DeadlineType.Closing;
        public DateTime DueAt { get; set; }
        public string Priority { get; set; } = DeadlinePriority.Medium;
        public bool AddedToCalendar { get; set; }
        public string? CalendarEventId { get; set; }
        // Bit flags for the T-7 / T-3 / T-1 tiers, so a reminder is never sent twice.
        public int RemindersSent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Tender? Tender { get; set; }
    }

    public static class DeadlineType
    {
        public const string Closing = "Closing";
        public const string Briefing = "Briefing";
        public const string Clarification = "Clarification";
        public const string Submission = "Submission";

        public static readonly string[] All = { Closing, Briefing, Clarification, Submission };

        public static bool IsValid(string? type) => type is not null && All.Contains(type);
    }

    public static class DeadlinePriority
    {
        public const string High = "high";
        public const string Medium = "medium";
        public const string Low = "low";

        public static readonly string[] All = { High, Medium, Low };

        public static bool IsValid(string? priority) => priority is not null && All.Contains(priority);
    }

    // Per-user link to an external calendar. Kept as its own row (not a column on User) so a
    // second provider can be added without touching the auth tables.
    public class CalendarConnection
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Provider { get; set; } = "Google";
        public bool Connected { get; set; }
        public string? Account { get; set; }
        public DateTime? ConnectedAt { get; set; }

        public User? User { get; set; }
    }
}
