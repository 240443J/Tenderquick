using Microsoft.EntityFrameworkCore;
using TenderquickServer.Data;
using TenderquickServer.Models;
using TenderquickServer.Models.Inventory;

namespace TenderquickServer.Services
{
    public class EfInventoryService : IInventoryService
    {
        private readonly AppDbContext _db;
        private readonly IAuditService _audit;
        private readonly CurrentUser _user;

        public EfInventoryService(AppDbContext db, IAuditService audit, CurrentUser user)
        {
            _db = db;
            _audit = audit;
            _user = user;
        }

        public async Task<IEnumerable<EquipmentDto>> GetEquipmentAsync(string? category, string? search)
        {
            var query = _db.InventoryItems.AsNoTracking().Where(i => i.IsActive);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(i => i.Category == category);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(i => i.Name.Contains(s) || i.Category.Contains(s));
            }

            return await query
                .OrderByDescending(i => i.UpdatedAt)
                .ThenByDescending(i => i.Id)
                .Select(i => new EquipmentDto(
                    i.Id, i.Name, i.Category, i.Unit,
                    i.Prices.OrderByDescending(p => p.EffectiveFrom).ThenByDescending(p => p.Id)
                        .Select(p => p.UnitCost).FirstOrDefault(),
                    i.SupplierName, i.LastTenderRef, i.UpdatedAt))
                .ToListAsync();
        }

        public async Task<EquipmentDto?> GetEquipmentByIdAsync(int id)
        {
            var item = await _db.InventoryItems.AsNoTracking()
                .Include(i => i.Prices)
                .FirstOrDefaultAsync(i => i.Id == id);

            return item is null ? null : ToDto(item);
        }

        public async Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentRequest req)
        {
            var now = DateTime.UtcNow;
            var item = new InventoryItem
            {
                Name = (req.Name ?? "New item").Trim(),
                Category = string.IsNullOrWhiteSpace(req.Category) ? "General" : req.Category.Trim(),
                Unit = string.IsNullOrWhiteSpace(req.Unit) ? "each" : req.Unit.Trim(),
                SupplierName = req.SupplierName?.Trim(),
                LastTenderRef = req.LastTenderRef?.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
            };

            item.Prices.Add(new PriceHistory
            {
                UnitCost = req.UnitCost ?? 0m,
                EffectiveFrom = now,
                SourceTenderRef = req.LastTenderRef?.Trim(),
                CreatedByUserId = _user.Id,
                CreatedAt = now,
            });

            _db.InventoryItems.Add(item);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Inventory.Created", "InventoryItem", item.Id, new { item.Name, item.Category });
            return ToDto(item);
        }

        public async Task<EquipmentDto?> UpdateEquipmentAsync(int id, UpdateEquipmentRequest req)
        {
            var item = await _db.InventoryItems.Include(i => i.Prices).FirstOrDefaultAsync(i => i.Id == id);
            if (item is null) return null;

            if (!string.IsNullOrWhiteSpace(req.Name)) item.Name = req.Name.Trim();
            if (!string.IsNullOrWhiteSpace(req.Category)) item.Category = req.Category.Trim();
            if (!string.IsNullOrWhiteSpace(req.Unit)) item.Unit = req.Unit.Trim();
            if (req.SupplierName is not null) item.SupplierName = req.SupplierName.Trim();
            if (req.LastTenderRef is not null) item.LastTenderRef = req.LastTenderRef.Trim();

            if (req.UnitCost is not null && req.UnitCost.Value != CurrentCost(item))
            {
                // Repricing appends a version; the previous cost stays on the record so any
                // quotation drafted against it can still be explained.
                item.Prices.Add(new PriceHistory
                {
                    UnitCost = req.UnitCost.Value,
                    EffectiveFrom = DateTime.UtcNow,
                    SourceTenderRef = item.LastTenderRef,
                    CreatedByUserId = _user.Id,
                    CreatedAt = DateTime.UtcNow,
                });

                await _audit.LogAsync("Inventory.Repriced", "InventoryItem", item.Id,
                    new { item.Name, UnitCost = req.UnitCost.Value });
            }

            item.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return ToDto(item);
        }

        public async Task<bool> DeleteEquipmentAsync(int id)
        {
            var item = await _db.InventoryItems.FirstOrDefaultAsync(i => i.Id == id);
            if (item is null) return false;

            // Soft delete: hard-deleting would cascade away the price history that past
            // quotations were built from.
            item.IsActive = false;
            item.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Inventory.Deleted", "InventoryItem", id, new { item.Name });
            return true;
        }

        public async Task<IEnumerable<PriceHistoryDto>?> GetPriceHistoryAsync(int id)
        {
            if (!await _db.InventoryItems.AnyAsync(i => i.Id == id)) return null;

            return await _db.PriceHistories
                .AsNoTracking()
                .Where(p => p.InventoryItemId == id)
                .OrderByDescending(p => p.EffectiveFrom)
                .ThenByDescending(p => p.Id)
                .Select(p => new PriceHistoryDto(p.Id, p.UnitCost, p.EffectiveFrom, p.SourceTenderRef))
                .ToListAsync();
        }

        public async Task<CurrentPriceDto?> GetCurrentPriceAsync(int id)
        {
            var price = await _db.PriceHistories
                .AsNoTracking()
                .Where(p => p.InventoryItemId == id && p.EffectiveFrom <= DateTime.UtcNow)
                .OrderByDescending(p => p.EffectiveFrom)
                .ThenByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            return price is null ? null : new CurrentPriceDto(id, price.UnitCost, price.EffectiveFrom);
        }

        public async Task<EquipmentDto?> AddPriceAsync(int id, AddPriceRequest req)
        {
            var item = await _db.InventoryItems.Include(i => i.Prices).FirstOrDefaultAsync(i => i.Id == id);
            if (item is null) return null;

            item.Prices.Add(new PriceHistory
            {
                UnitCost = req.UnitCost,
                EffectiveFrom = req.EffectiveFrom ?? DateTime.UtcNow,
                SourceTenderRef = req.SourceTenderRef?.Trim() ?? item.LastTenderRef,
                CreatedByUserId = _user.Id,
                CreatedAt = DateTime.UtcNow,
            });

            if (!string.IsNullOrWhiteSpace(req.SourceTenderRef))
                item.LastTenderRef = req.SourceTenderRef.Trim();

            item.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Inventory.Repriced", "InventoryItem", item.Id, new { item.Name, req.UnitCost });
            return ToDto(item);
        }

        public async Task<IEnumerable<LabourDto>> GetLabourAsync() =>
            await _db.LabourRates
                .AsNoTracking()
                .Where(l => l.IsActive)
                .OrderByDescending(l => l.UpdatedAt)
                .ThenByDescending(l => l.Id)
                .Select(l => new LabourDto(
                    l.Id, l.Role, l.Unit,
                    l.Rates.OrderByDescending(r => r.EffectiveFrom).ThenByDescending(r => r.Id)
                        .Select(r => r.HourlyRate).FirstOrDefault(),
                    l.UpdatedAt))
                .ToListAsync();

        public async Task<LabourDto> CreateLabourAsync(CreateLabourRequest req)
        {
            var now = DateTime.UtcNow;
            var labour = new LabourRate
            {
                Role = (req.Role ?? "New role").Trim(),
                Unit = string.IsNullOrWhiteSpace(req.Unit) ? "hour" : req.Unit.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
            };

            labour.Rates.Add(new LabourRateHistory
            {
                HourlyRate = req.Rate ?? 0m,
                EffectiveFrom = now,
                CreatedByUserId = _user.Id,
                CreatedAt = now,
            });

            _db.LabourRates.Add(labour);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Labour.Created", "LabourRate", labour.Id, new { labour.Role });
            return ToDto(labour);
        }

        public async Task<LabourDto?> UpdateLabourAsync(int id, UpdateLabourRequest req)
        {
            var labour = await _db.LabourRates.Include(l => l.Rates).FirstOrDefaultAsync(l => l.Id == id);
            if (labour is null) return null;

            if (!string.IsNullOrWhiteSpace(req.Role)) labour.Role = req.Role.Trim();
            if (!string.IsNullOrWhiteSpace(req.Unit)) labour.Unit = req.Unit.Trim();

            if (req.Rate is not null && req.Rate.Value != CurrentRate(labour))
            {
                labour.Rates.Add(new LabourRateHistory
                {
                    HourlyRate = req.Rate.Value,
                    EffectiveFrom = DateTime.UtcNow,
                    CreatedByUserId = _user.Id,
                    CreatedAt = DateTime.UtcNow,
                });

                await _audit.LogAsync("Labour.Repriced", "LabourRate", labour.Id,
                    new { labour.Role, Rate = req.Rate.Value });
            }

            labour.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return ToDto(labour);
        }

        public async Task<bool> DeleteLabourAsync(int id)
        {
            var labour = await _db.LabourRates.FirstOrDefaultAsync(l => l.Id == id);
            if (labour is null) return false;

            labour.IsActive = false;
            labour.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Labour.Deleted", "LabourRate", id, new { labour.Role });
            return true;
        }

        public async Task<IEnumerable<LabourRateHistoryDto>?> GetLabourHistoryAsync(int id)
        {
            if (!await _db.LabourRates.AnyAsync(l => l.Id == id)) return null;

            return await _db.LabourRateHistories
                .AsNoTracking()
                .Where(r => r.LabourRateId == id)
                .OrderByDescending(r => r.EffectiveFrom)
                .ThenByDescending(r => r.Id)
                .Select(r => new LabourRateHistoryDto(r.Id, r.HourlyRate, r.EffectiveFrom))
                .ToListAsync();
        }

        private static decimal CurrentCost(InventoryItem item) =>
            item.Prices.OrderByDescending(p => p.EffectiveFrom).ThenByDescending(p => p.Id)
                .Select(p => p.UnitCost).FirstOrDefault();

        private static decimal CurrentRate(LabourRate labour) =>
            labour.Rates.OrderByDescending(r => r.EffectiveFrom).ThenByDescending(r => r.Id)
                .Select(r => r.HourlyRate).FirstOrDefault();

        private static EquipmentDto ToDto(InventoryItem i) => new(
            i.Id, i.Name, i.Category, i.Unit, CurrentCost(i), i.SupplierName, i.LastTenderRef, i.UpdatedAt);

        private static LabourDto ToDto(LabourRate l) => new(
            l.Id, l.Role, l.Unit, CurrentRate(l), l.UpdatedAt);
    }
}
