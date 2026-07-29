using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class GradesController : ControllerBase
	{
		private readonly IGradeService _gradeService;

		public GradesController(IGradeService gradeService)
		{
			_gradeService = gradeService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll([FromQuery] bool? isDeleted = false)
		{
			var grades = await _gradeService.GetAllAsync(isDeleted);
			return Ok(grades);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var grade = await _gradeService.GetByIdAsync(id);
			if (grade == null) return NotFound();
			return Ok(grade);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateGradeRequest request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var created = await _gradeService.CreateAsync(request);
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGradeRequest request)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var updated = await _gradeService.UpdateAsync(id, request);
			return Ok(updated);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var success = await _gradeService.SoftDeleteAsync(id);
			if (!success) return NotFound();
			return NoContent();
		}
	}
}