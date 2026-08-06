using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenderquickServer.Models;
using TenderquickServer.Models.Deadlines;
using TenderquickServer.Services;

namespace TenderquickServer.Controllers
{
    [ApiController]
    [Route("api/deadlines")]
    [Authorize]
    public class DeadlinesController : ControllerBase
    {
        private readonly IDeadlineService _deadlines;
        private readonly ICalendarService _calendar;
        private readonly CurrentUser _user;

        public DeadlinesController(IDeadlineService deadlines, ICalendarService calendar, CurrentUser user)
        {
            _deadlines = deadlines;
            _calendar = calendar;
            _user = user;
        }

        // GET api/deadlines?tenderId=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeadlineDto>>> GetAll([FromQuery] int? tenderId)
        {
            return Ok(await _deadlines.GetAllAsync(tenderId));
        }

        // POST api/deadlines
        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
        public async Task<ActionResult<DeadlineDto>> Create([FromBody] CreateDeadlineRequest req)
        {
            var result = await _deadlines.CreateAsync(req);
            return result.Outcome switch
            {
                DeadlineOutcome.Ok => CreatedAtAction(nameof(GetAll), new { tenderId = req.TenderId }, result.Deadline),
                DeadlineOutcome.TenderNotFound => NotFound(new { message = "Tender not found." }),
                DeadlineOutcome.InvalidType => BadRequest(new { message = "Invalid deadline type." }),
                _ => BadRequest(),
            };
        }

        // PUT api/deadlines/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
        public async Task<ActionResult<DeadlineDto>> Update(int id, [FromBody] UpdateDeadlineRequest req)
        {
            var result = await _deadlines.UpdateAsync(id, req);
            return result.Outcome switch
            {
                DeadlineOutcome.Ok => Ok(result.Deadline),
                DeadlineOutcome.NotFound => NotFound(),
                DeadlineOutcome.InvalidType => BadRequest(new { message = "Invalid deadline type." }),
                _ => BadRequest(),
            };
        }

        // DELETE api/deadlines/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
        public async Task<IActionResult> Delete(int id)
        {
            var outcome = await _deadlines.DeleteAsync(id);
            return outcome == DeadlineOutcome.Ok ? NoContent() : NotFound();
        }

        // GET api/deadlines/calendar
        [HttpGet("calendar")]
        public async Task<ActionResult<CalendarStatusDto>> GetCalendar()
        {
            var userId = _user.Id;
            if (userId is null) return Unauthorized();

            return Ok(await _calendar.GetStatusAsync(userId.Value));
        }

        // POST api/deadlines/calendar/connect
        [HttpPost("calendar/connect")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
        public async Task<ActionResult<CalendarStatusDto>> ConnectCalendar()
        {
            var userId = _user.Id;
            if (userId is null) return Unauthorized();

            return Ok(await _calendar.ConnectAsync(userId.Value, _user.Email));
        }

        // POST api/deadlines/calendar/disconnect
        [HttpPost("calendar/disconnect")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
        public async Task<ActionResult<CalendarStatusDto>> DisconnectCalendar()
        {
            var userId = _user.Id;
            if (userId is null) return Unauthorized();

            return Ok(await _calendar.DisconnectAsync(userId.Value));
        }

        // POST api/deadlines/{id}/calendar
        [HttpPost("{id}/calendar")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
        public async Task<ActionResult<DeadlineDto>> AddToCalendar(int id)
        {
            var userId = _user.Id;
            if (userId is null) return Unauthorized();

            var result = await _deadlines.AddToCalendarAsync(id, userId.Value);
            return result.Outcome switch
            {
                DeadlineOutcome.Ok => Ok(result.Deadline),
                DeadlineOutcome.NotFound => NotFound(),
                DeadlineOutcome.CalendarNotConnected =>
                    BadRequest(new { message = "Connect a calendar before syncing events." }),
                _ => BadRequest(),
            };
        }

        // POST api/deadlines/calendar/sync-all
        [HttpPost("calendar/sync-all")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
        public async Task<ActionResult<IEnumerable<DeadlineDto>>> SyncAll()
        {
            var userId = _user.Id;
            if (userId is null) return Unauthorized();

            return Ok(await _deadlines.SyncAllToCalendarAsync(userId.Value));
        }
    }
}
