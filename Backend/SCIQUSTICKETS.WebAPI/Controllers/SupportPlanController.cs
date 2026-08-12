using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.BUSINESS.BusinessModels.SupportPlanDTOs;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SupportPlanController : ControllerBase
    {
        private readonly ISupportPlanService _supportPlanService;

        public SupportPlanController(ISupportPlanService supportPlanService)
        {
            _supportPlanService = supportPlanService;
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")] // Using custom authorization policy if available, otherwise just use Authorize
        public async Task<ActionResult<SupportPlanResponse>> Create(CreateSupportPlanRequest request)
        {
            var userId = User.Identity?.Name ?? "SYSTEM";
            var result = await _supportPlanService.CreatePlanAsync(request, userId);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<SupportPlanResponse>> Update(Guid id, UpdateSupportPlanRequest request)
        {
            var userId = User.Identity?.Name ?? "SYSTEM";
            var result = await _supportPlanService.UpdatePlanAsync(id, request, userId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<SupportPlanResponse>>> GetAll()
        {
            var result = await _supportPlanService.GetAllPlansAsync();
            return Ok(result);
        }

        [HttpPost("assign")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<AccountSupportPlanResponse>> AssignPlan(AssignPlanRequest request)
        {
            var userId = User.Identity?.Name ?? "SYSTEM";
            var result = await _supportPlanService.AssignPlanToAccountAsync(request, userId);
            return Ok(result);
        }

        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<List<AccountSupportPlanResponse>>> GetAccountPlans(string accountId)
        {
            var result = await _supportPlanService.GetAccountPlansAsync(accountId);
            return Ok(result);
        }
    }
}
