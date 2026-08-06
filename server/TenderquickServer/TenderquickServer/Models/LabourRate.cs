namespace TenderquickServer.Models
{
    public class LabourRate
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Unit { get; set; } = "hour";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<LabourRateHistory> Rates { get; set; } = new List<LabourRateHistory>();
    }

    // Versioned exactly like PriceHistory — see the comment there.
    public class LabourRateHistory
    {
        public int Id { get; set; }
        public int LabourRateId { get; set; }
        public decimal HourlyRate { get; set; }
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public LabourRate? LabourRate { get; set; }
    }
}
