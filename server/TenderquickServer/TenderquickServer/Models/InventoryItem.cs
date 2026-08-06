namespace TenderquickServer.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Unit { get; set; } = "each";
        public string? SupplierName { get; set; }
        public string? LastTenderRef { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PriceHistory> Prices { get; set; } = new List<PriceHistory>();
    }

    // Prices are append-only: a repriced item gets a new row, never an UPDATE. The current
    // price is the newest row with EffectiveFrom <= now, so past quotes stay explainable.
    public class PriceHistory
    {
        public int Id { get; set; }
        public int InventoryItemId { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public string? SourceTenderRef { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public InventoryItem? InventoryItem { get; set; }
    }
}
