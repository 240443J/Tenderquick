using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenderquickServer.Models;
using TenderquickServer.Models.Tenders;
using TenderquickServer.Services;

namespace TenderquickServer.Controllers
{
    [ApiController]
    [Route("api/tenders")]
    [Authorize]
    public class TendersController : ControllerBase
    {
        private readonly ITenderService _tenders;

        public TendersController(ITenderService tenders)
        {
            _tenders = tenders;
        }

        // GET api/tenders?status=&search=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TenderListItem>>> GetAll(
            [FromQuery] string? status, [FromQuery] string? search)
        {
            if (!string.IsNullOrWhiteSpace(status) && !TenderStatus.IsValid(status))
                return BadRequest(new { message = $"Invalid status '{status}'." });

            return Ok(await _tenders.GetAllAsync(status, search));
        }

        // GET api/tenders/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tender>> GetById(int id)
        {
            var tender = await _tenders.GetByIdAsync(id);
            return tender is null ? NotFound() : Ok(tender);
        }

        // POST api/tenders
        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
        public async Task<ActionResult<Tender>> Create([FromBody] CreateTenderRequest req)
        {
            try
            {
                var tender = await _tenders.CreateAsync(req);
                return CreatedAtAction(nameof(GetById), new { id = tender.Id }, tender);
            }
            catch (DuplicateReferenceException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT api/tenders/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
        public async Task<ActionResult<Tender>> Update(int id, [FromBody] UpdateTenderRequest req)
        {
            if (!TenderStatus.IsValid(req.Status))
                return BadRequest(new { message = $"Invalid status '{req.Status}'." });

            var tender = await _tenders.UpdateAsync(id, req);
            return tender is null ? NotFound() : Ok(tender);
        }

        // DELETE api/tenders/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _tenders.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
