using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	[ApiController]
	[Route("api/email-test")]
	[Authorize]
	public class EmailTestController : ControllerBase
	{
		private readonly IEmployeeEmailNotificationService _emailService;

		public EmailTestController(
			IEmployeeEmailNotificationService emailService)
		{
			_emailService = emailService;
		}

		[HttpPost("{ticketId}")]
		public async Task<IActionResult> SendTestEmail(
			Guid ticketId,
			[FromQuery] string eventType = "Assigned")
		{
			await _emailService.SendTicketNotificationAsync(
				ticketId,
				eventType,
				User.Identity?.Name,
				"Test email notification");

			return Ok(new
			{
				message = "Test email notification triggered.",
				ticketId,
				eventType
			});
		}
	}
}