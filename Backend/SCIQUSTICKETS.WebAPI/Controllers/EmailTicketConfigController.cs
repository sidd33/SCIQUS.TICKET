using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "emailticket.config")]
    public class EmailTicketConfigController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmailTicketConfigController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EmailTicketConfig
        [HttpGet]
        public async Task<IActionResult> GetConfig()
        {
            var config = await _context.EmailTicketConfigs.FirstOrDefaultAsync();
            if (config == null) return NotFound();
            
            // Do not return encrypted tokens!
            return Ok(new
            {
                config.EmailTicketConfigId,
                config.Provider,
                config.EmailAddress,
                config.IsEnabled,
                config.AutoCreateEnabled,
                config.PollingIntervalMinutes,
                config.IsAuthValid,
                config.LastPollDate,
                config.LastError,
                config.DefaultPriorityId,
                config.DefaultBusinessImpactId,
                config.DefaultDepartmentId,
                config.DefaultTicketTypeId,
                config.DefaultTicketSubTypeId,
                config.DefaultAssigneeId
            });
        }

        // POST: api/EmailTicketConfig
        [HttpPost]
        public async Task<IActionResult> SaveConfig([FromBody] EmailTicketConfig requestConfig)
        {
            // Placeholder: validate and map specific DTO
            var existing = await _context.EmailTicketConfigs.FirstOrDefaultAsync();
            if (existing == null)
            {
                requestConfig.EmailTicketConfigId = Guid.NewGuid();
                _context.EmailTicketConfigs.Add(requestConfig);
            }
            else
            {
                existing.Provider = requestConfig.Provider;
                existing.EmailAddress = requestConfig.EmailAddress;
                existing.IsEnabled = requestConfig.IsEnabled;
                existing.AutoCreateEnabled = requestConfig.AutoCreateEnabled;
                existing.PollingIntervalMinutes = requestConfig.PollingIntervalMinutes;
                
                existing.DefaultPriorityId = requestConfig.DefaultPriorityId;
                existing.DefaultBusinessImpactId = requestConfig.DefaultBusinessImpactId;
                existing.DefaultDepartmentId = requestConfig.DefaultDepartmentId;
                existing.DefaultTicketTypeId = requestConfig.DefaultTicketTypeId;
                existing.DefaultTicketSubTypeId = requestConfig.DefaultTicketSubTypeId;
                existing.DefaultAssigneeId = requestConfig.DefaultAssigneeId;
                
                // Note: updating tokens would be handled via a separate OAuth callback endpoint
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: api/EmailTicketConfig/InboxReview
        [HttpGet("InboxReview")]
        [Authorize(Policy = "emailticket.review")]
        public async Task<IActionResult> GetInboxReview([FromQuery] string status = "Pending")
        {
            var messages = await _context.EmailInboxMessages
                .Where(m => m.ProcessingStatus == status)
                .OrderByDescending(m => m.EmailReceivedDate)
                .ToListAsync();
            return Ok(messages);
        }
    }
}
