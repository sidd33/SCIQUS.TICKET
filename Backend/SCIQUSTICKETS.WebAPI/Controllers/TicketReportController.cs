// SCIQUSTICKETS.WebAPI/Controllers/TicketReportController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	// TODO: add [Authorize(Policy = "ticket.report.view" / "ticket.report.all")] once policies are wired.
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class TicketReportController : ControllerBase
	{
		private readonly ITicketReportService _reportService;

		public TicketReportController(ITicketReportService reportService)
		{
			_reportService = reportService;
		}

		[HttpGet("summary")]
		public async Task<IActionResult> GetSummary([FromQuery] TicketReportQueryParams query)
			=> Ok(await _reportService.GetSummaryAsync(query));

		[HttpGet("by-department")]
		public async Task<IActionResult> GetByDepartment([FromQuery] TicketReportQueryParams query)
			=> Ok(await _reportService.GetCountsByDepartmentAsync(query));

		[HttpGet("by-priority")]
		public async Task<IActionResult> GetByPriority([FromQuery] TicketReportQueryParams query)
			=> Ok(await _reportService.GetCountsByPriorityAsync(query));

		[HttpGet("by-type")]
		public async Task<IActionResult> GetByType([FromQuery] TicketReportQueryParams query)
			=> Ok(await _reportService.GetCountsByTypeAsync(query));

		[HttpGet("by-status")]
		public async Task<IActionResult> GetByStatus([FromQuery] TicketReportQueryParams query)
			=> Ok(await _reportService.GetCountsByStatusAsync(query));

		[HttpGet("sla-compliance")]
		public async Task<IActionResult> GetSlaCompliance([FromQuery] TicketReportQueryParams query)
			=> Ok(await _reportService.GetSlaComplianceAsync(query));

		[HttpGet("reopen-rate")]
		public async Task<IActionResult> GetReopenRate([FromQuery] TicketReportQueryParams query)
			=> Ok(await _reportService.GetReopenRateAsync(query));

		[HttpGet("agent-workload")]
		public async Task<IActionResult> GetAgentWorkload([FromQuery] TicketReportQueryParams query)
			=> Ok(await _reportService.GetAgentWorkloadAsync(query));

		[HttpGet("channel-mix")]
		public async Task<IActionResult> GetChannelMix([FromQuery] TicketReportQueryParams query)
			=> Ok(await _reportService.GetChannelMixAsync(query));


	}
}