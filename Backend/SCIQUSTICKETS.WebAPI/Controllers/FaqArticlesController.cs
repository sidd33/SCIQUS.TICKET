// SCIQUSTICKETS.WebAPI/Controllers/FaqArticlesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketMasterRequestDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using System.Security.Claims;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	// TODO: add [Authorize(Policy = "ticketmaster.view" / "ticketmaster.manage")] once wired.
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class FaqArticlesController : ControllerBase
	{
		private readonly IFaqArticleService _faqService;

		public FaqArticlesController(IFaqArticleService faqService)
		{
			_faqService = faqService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
			=> Ok(await _faqService.GetAllAsync(includeInactive));

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var result = await _faqService.GetByIdAsync(id);
			return result == null ? NotFound() : Ok(result);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateFaqArticleRequest request)
		{
			var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
			var result = await _faqService.CreateAsync(request, actorId);
			return CreatedAtAction(nameof(GetById), new { id = result.FaqArticleId }, result);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFaqArticleRequest request)
		{
			var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
			var result = await _faqService.UpdateAsync(id, request, actorId);
			return result == null ? NotFound() : Ok(result);
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var deleted = await _faqService.SoftDeleteAsync(id);
			return deleted ? Ok(new { Message = "FAQ article deleted." }) : NotFound();
		}
	}
}