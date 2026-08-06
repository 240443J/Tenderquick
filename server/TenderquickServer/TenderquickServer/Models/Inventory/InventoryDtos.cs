namespace TenderquickServer.Models.Inventory
{
    public record EquipmentDto(
        int Id,
        string Name,
        string Category,
        string Unit,
        decimal UnitCost,
        string? SupplierName,
        string? LastTenderRef,
        DateTime UpdatedAt);

    public record CreateEquipmentRequest(
        string Name,
        string? Category,
        string? Unit,
        decimal? UnitCost,
        string? SupplierName,
        string? LastTenderRef);

    // Every field is optional: the inventory grid saves one cell at a time, so an absent
    // field means "leave alone" and must not be read as "set to null/zero".
    public record UpdateEquipmentRequest(
        string? Name,
        string? Category,
        string? Unit,
        decimal? UnitCost,
        string? SupplierName,
        string? LastTenderRef);

    public record PriceHistoryDto(
        int Id,
        decimal UnitCost,
        DateTime EffectiveFrom,
        string? SourceTenderRef);

    public record AddPriceRequest(decimal UnitCost, DateTime? EffectiveFrom, string? SourceTenderRef);

    public record CurrentPriceDto(int InventoryItemId, decimal UnitCost, DateTime EffectiveFrom);

    public record LabourDto(int Id, string Role, string Unit, decimal Rate, DateTime UpdatedAt);

    public record CreateLabourRequest(string Role, string? Unit, decimal? Rate);

    public record UpdateLabourRequest(string? Role, string? Unit, decimal? Rate);

    public record LabourRateHistoryDto(int Id, decimal HourlyRate, DateTime EffectiveFrom);
}
