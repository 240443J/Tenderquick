using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenderquickServer.Models;
using TenderquickServer.Models.Tenders;
using TenderquickServer.Services;

namespace TenderquickServer.Controllers
{
    [ApiController]
    [Route("api/tenders")]
    [Authorize] // any authenticated role can read; mutations are gated per-method
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
            var items = await _tenders.GetAllAsync(status, search);
            return Ok(items);
        }

        // GET api/tenders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TenderDetail>> GetById(int id)
        {
            var tender = await _tenders.GetByIdAsync(id);
            return tender is null ? NotFound() : Ok(tender);
        }

        // POST api/tenders
        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
        public async Task<ActionResult<TenderDetail>> Create([FromBody] CreateTenderRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Reference) || string.IsNullOrWhiteSpace(req.Title))
                return BadRequest(new { message = "Reference and title are required." });

            var result = await _tenders.CreateAsync(req);
            return result.Outcome switch
            {
                CreateOutcome.Created => CreatedAtAction(nameof(GetById), new { id = result.Tender!.Id }, result.Tender),
                CreateOutcome.DuplicateReference => Conflict(new { message = "A tender with this reference already exists." }),
                _ => BadRequest(),
            };
        }

        // PUT api/tenders/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
        public async Task<ActionResult<TenderDetail>> Update(int id, [FromBody] UpdateTenderRequest req)
        {
            var result = await _tenders.UpdateAsync(id, req);
            return result.Outcome switch
            {
                UpdateOutcome.Updated => Ok(result.Tender),
                UpdateOutcome.NotFound => NotFound(),
                UpdateOutcome.InvalidStatus => BadRequest(new { message = "Invalid status value." }),
                _ => BadRequest(),
            };
        }

        // DELETE api/tenders/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var outcome = await _tenders.DeleteAsync(id);
            return outcome switch
            {
                DeleteOutcome.Deleted => NoContent(),
                DeleteOutcome.NotFound => NotFound(),
                DeleteOutcome.HasQuotations => Conflict(new
                {
                    message = "This tender has quotations attached. Delete or reassign them first.",
                }),
                _ => BadRequest(),
            };
        }
    }
}
