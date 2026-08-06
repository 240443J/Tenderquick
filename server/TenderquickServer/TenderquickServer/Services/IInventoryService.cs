using TenderquickServer.Models.Inventory;

namespace TenderquickServer.Services
{
    public interface IInventoryService
    {
        Task<IEnumerable<EquipmentDto>> GetEquipmentAsync(string? category, string? search);
        Task<EquipmentDto?> GetEquipmentByIdAsync(int id);
        Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentRequest req);
        Task<EquipmentDto?> UpdateEquipmentAsync(int id, UpdateEquipmentRequest req);
        Task<bool> DeleteEquipmentAsync(int id);
        Task<IEnumerable<PriceHistoryDto>?> GetPriceHistoryAsync(int id);
        Task<CurrentPriceDto?> GetCurrentPriceAsync(int id);
        Task<EquipmentDto?> AddPriceAsync(int id, AddPriceRequest req);

        Task<IEnumerable<LabourDto>> GetLabourAsync();
        Task<LabourDto> CreateLabourAsync(CreateLabourRequest req);
        Task<LabourDto?> UpdateLabourAsync(int id, UpdateLabourRequest req);
        Task<bool> DeleteLabourAsync(int id);
        Task<IEnumerable<LabourRateHistoryDto>?> GetLabourHistoryAsync(int id);
    }
}
