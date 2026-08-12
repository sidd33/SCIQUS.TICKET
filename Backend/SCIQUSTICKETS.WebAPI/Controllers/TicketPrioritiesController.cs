using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using System.Security.Claims;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    // TODO: add [Authorize(Policy = "ticketmaster.view" / "ticketmaster.manage")] per action
    // once those policies are registered.
    [ApiController]
    [Route("api/TicketPriorities")]
    [Authorize]
    public class TicketPrioritiesController : ControllerBase
    {
        private readonly ITicketPriorityService _ticketPriorityService;

        public TicketPrioritiesController(ITicketPriorityService ticketPriorityService)
        {
            _ticketPriorityService = ticketPriorityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TicketMasterQueryParams queryParams)
        {
            var result = await _ticketPriorityService.GetAllAsync(queryParams);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _ticketPriorityService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = $"Ticket Priority with ID {id} not found." });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTicketPriorityRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(actorUserId)) return Unauthorized();

            try
            {
                var created = await _ticketPriorityService.CreateAsync(request, actorUserId);
                return CreatedAtAction(nameof(GetById), new { id = created.TicketPriorityId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTicketPriorityRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(actorUserId)) return Unauthorized();

            try
            {
                var updated = await _ticketPriorityService.UpdateAsync(id, request, actorUserId);
                if (updated == null) return NotFound(new { message = $"Ticket Priority with ID {id} not found." });

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> SetStatus(Guid id, [FromQuery] bool status)
        {
            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(actorUserId)) return Unauthorized();

            try
            {
                var updated = await _ticketPriorityService.SetStatusAsync(id, status, actorUserId);
                if (!updated) return NotFound(new { message = $"Ticket Priority with ID {id} not found." });

                return Ok(new { message = $"Ticket Priority {id} status set to {status}." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

		[HttpDelete("{id}")]
		public async Task<IActionResult> SoftDelete(Guid id)
		{
			var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(actorUserId))
				return Unauthorized();

			try
			{
				var deleted = await _ticketPriorityService
					.SoftDeleteAsync(id, actorUserId);
				if (!deleted) return NotFound(new { message = $"Ticket Priority with ID {id} not found." });

                return Ok(new { message = $"Ticket Priority {id} soft-deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
