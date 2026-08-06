using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenderquickServer.Models;
using TenderquickServer.Models.Inventory;
using TenderquickServer.Services;

namespace TenderquickServer.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [Authorize] // Viewer may read prices; mutations are gated per-method
    public class InventoryController : ControllerBase
    {
        private const string Editors = $"{Roles.Admin},{Roles.Estimator}";

        private readonly IInventoryService _inventory;

        public InventoryController(IInventoryService inventory)
        {
            _inventory = inventory;
        }

        // GET api/inventory/equipment?category=&search=
        [HttpGet("equipment")]
        public async Task<ActionResult<IEnumerable<EquipmentDto>>> GetEquipment(
            [FromQuery] string? category, [FromQuery] string? search)
        {
            return Ok(await _inventory.GetEquipmentAsync(category, search));
        }

        // GET api/inventory/equipment/{id}
        [HttpGet("equipment/{id}")]
        public async Task<ActionResult<EquipmentDto>> GetEquipmentById(int id)
        {
            var item = await _inventory.GetEquipmentByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        // POST api/inventory/equipment
        [HttpPost("equipment")]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<EquipmentDto>> CreateEquipment([FromBody] CreateEquipmentRequest req)
        {
            var item = await _inventory.CreateEquipmentAsync(req);
            return CreatedAtAction(nameof(GetEquipmentById), new { id = item.Id }, item);
        }

        // PUT api/inventory/equipment/{id}
        [HttpPut("equipment/{id}")]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<EquipmentDto>> UpdateEquipment(int id, [FromBody] UpdateEquipmentRequest req)
        {
            var item = await _inventory.UpdateEquipmentAsync(id, req);
            return item is null ? NotFound() : Ok(item);
        }

        // DELETE api/inventory/equipment/{id}
        [HttpDelete("equipment/{id}")]
        [Authorize(Roles = Editors)]
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            return await _inventory.DeleteEquipmentAsync(id) ? NoContent() : NotFound();
        }

        // GET api/inventory/equipment/{id}/price-history
        [HttpGet("equipment/{id}/price-history")]
        public async Task<ActionResult<IEnumerable<PriceHistoryDto>>> GetPriceHistory(int id)
        {
            var history = await _inventory.GetPriceHistoryAsync(id);
            return history is null ? NotFound() : Ok(history);
        }

        // GET api/inventory/equipment/{id}/current-price
        [HttpGet("equipment/{id}/current-price")]
        public async Task<ActionResult<CurrentPriceDto>> GetCurrentPrice(int id)
        {
            var price = await _inventory.GetCurrentPriceAsync(id);
            return price is null ? NotFound() : Ok(price);
        }

        // POST api/inventory/equipment/{id}/prices
        [HttpPost("equipment/{id}/prices")]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<EquipmentDto>> AddPrice(int id, [FromBody] AddPriceRequest req)
        {
            var item = await _inventory.AddPriceAsync(id, req);
            return item is null ? NotFound() : Ok(item);
        }

        // GET api/inventory/labour
        [HttpGet("labour")]
        public async Task<ActionResult<IEnumerable<LabourDto>>> GetLabour()
        {
            return Ok(await _inventory.GetLabourAsync());
        }

        // POST api/inventory/labour
        [HttpPost("labour")]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<LabourDto>> CreateLabour([FromBody] CreateLabourRequest req)
        {
            var labour = await _inventory.CreateLabourAsync(req);
            return CreatedAtAction(nameof(GetLabour), null, labour);
        }

        // PUT api/inventory/labour/{id}
        [HttpPut("labour/{id}")]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<LabourDto>> UpdateLabour(int id, [FromBody] UpdateLabourRequest req)
        {
            var labour = await _inventory.UpdateLabourAsync(id, req);
            return labour is null ? NotFound() : Ok(labour);
        }

        // DELETE api/inventory/labour/{id}
        [HttpDelete("labour/{id}")]
        [Authorize(Roles = Editors)]
        public async Task<IActionResult> DeleteLabour(int id)
        {
            return await _inventory.DeleteLabourAsync(id) ? NoContent() : NotFound();
        }

        // GET api/inventory/labour/{id}/history
        [HttpGet("labour/{id}/history")]
        public async Task<ActionResult<IEnumerable<LabourRateHistoryDto>>> GetLabourHistory(int id)
        {
            var history = await _inventory.GetLabourHistoryAsync(id);
            return history is null ? NotFound() : Ok(history);
        }
    }
}
