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
    [Route("api/TicketSubTypes")]
    [Authorize]
    public class TicketSubTypesController : ControllerBase
    {
        private readonly ITicketSubTypeService _ticketSubTypeService;

        public TicketSubTypesController(ITicketSubTypeService ticketSubTypeService)
        {
            _ticketSubTypeService = ticketSubTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TicketSubTypeQueryParams queryParams)
        {
            var result = await _ticketSubTypeService.GetAllAsync(queryParams);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _ticketSubTypeService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = $"Ticket Sub-Type with ID {id} not found." });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTicketSubTypeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(actorUserId)) return Unauthorized();

            try
            {
                var created = await _ticketSubTypeService.CreateAsync(request, actorUserId);
                return CreatedAtAction(nameof(GetById), new { id = created.TicketSubTypeId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTicketSubTypeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var actorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(actorUserId)) return Unauthorized();

            try
            {
                var updated = await _ticketSubTypeService.UpdateAsync(id, request, actorUserId);
                if (updated == null) return NotFound(new { message = $"Ticket Sub-Type with ID {id} not found." });

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
                var updated = await _ticketSubTypeService.SetStatusAsync(id, status, actorUserId);
                if (!updated) return NotFound(new { message = $"Ticket Sub-Type with ID {id} not found." });

                return Ok(new { message = $"Ticket Sub-Type {id} status set to {status}." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            try
            {
                var deleted = await _ticketSubTypeService.SoftDeleteAsync(id);
                if (!deleted) return NotFound(new { message = $"Ticket Sub-Type with ID {id} not found." });

                return Ok(new { message = $"Ticket Sub-Type {id} soft-deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
