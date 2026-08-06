using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenderquickServer.Models;
using TenderquickServer.Models.Quotations;
using TenderquickServer.Services;

namespace TenderquickServer.Controllers
{
    [ApiController]
    [Route("api/quotations")]
    [Authorize]
    public class QuotationsController : ControllerBase
    {
        private const string Editors = $"{Roles.Admin},{Roles.Estimator}";

        private readonly IQuotationService _quotations;

        public QuotationsController(IQuotationService quotations)
        {
            _quotations = quotations;
        }

        // GET api/quotations?tenderId=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuotationDto>>> GetAll([FromQuery] int? tenderId)
        {
            return Ok(await _quotations.GetAllAsync(tenderId));
        }

        // GET api/quotations/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<QuotationDto>> GetById(int id)
        {
            var quote = await _quotations.GetByIdAsync(id);
            return quote is null ? NotFound() : Ok(quote);
        }

        // POST api/quotations/generate/{tenderId}
        [HttpPost("generate/{tenderId}")]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<QuotationDto>> Generate(int tenderId, CancellationToken ct)
        {
            var result = await _quotations.GenerateFromTenderAsync(tenderId, ct);
            return result.Outcome switch
            {
                QuotationOutcome.Ok => CreatedAtAction(nameof(GetById), new { id = result.Quotation!.Id }, result.Quotation),
                QuotationOutcome.TenderNotFound => NotFound(new { message = "Tender not found." }),
                QuotationOutcome.EmptyCatalog => BadRequest(new
                {
                    message = "Add equipment or labour rates to the inventory before drafting a quotation.",
                }),
                _ => BadRequest(),
            };
        }

        // PUT api/quotations/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<QuotationDto>> Update(int id, [FromBody] UpdateQuotationRequest req)
        {
            var result = await _quotations.UpdateAsync(id, req);
            return result.Outcome switch
            {
                QuotationOutcome.Ok => Ok(result.Quotation),
                QuotationOutcome.NotFound => NotFound(),
                _ => BadRequest(),
            };
        }

        // POST api/quotations/{id}/verify
        // The human-in-the-loop gate: the signer is taken from the token, never from the body.
        [HttpPost("{id}/verify")]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<QuotationDto>> Verify(int id)
        {
            var result = await _quotations.VerifyAsync(id);
            return result.Outcome switch
            {
                QuotationOutcome.Ok => Ok(result.Quotation),
                QuotationOutcome.AlreadyVerified => Ok(result.Quotation),
                QuotationOutcome.NotFound => NotFound(),
                _ => BadRequest(),
            };
        }

        // GET api/quotations/{id}/signoffs
        [HttpGet("{id}/signoffs")]
        public async Task<ActionResult<IEnumerable<SignoffDto>>> GetSignoffs(int id)
        {
            var signoffs = await _quotations.GetSignoffsAsync(id);
            return signoffs is null ? NotFound() : Ok(signoffs);
        }

        // DELETE api/quotations/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            return await _quotations.DeleteAsync(id) ? NoContent() : NotFound();
        }
    }
}
