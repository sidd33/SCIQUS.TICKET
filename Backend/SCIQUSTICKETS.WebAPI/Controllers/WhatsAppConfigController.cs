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
    [Authorize]
    public class WhatsAppConfigController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WhatsAppConfigController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetConfig()
        {
            var config = await _context.WhatsAppChannelConfigs.FirstOrDefaultAsync();
            if (config == null) return NotFound();
            
            return Ok(new
            {
                config.WhatsAppChannelConfigId,
                config.Provider,
                config.BusinessPhoneNumberId,
                config.IsEnabled,
                config.AutoCreateEnabled,
                config.IsAuthValid,
                config.LastError,
                config.DefaultPriorityId,
                config.DefaultBusinessImpactId,
                config.DefaultDepartmentId,
                config.DefaultTicketTypeId,
                config.DefaultTicketSubTypeId,
                config.DefaultAssigneeId
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveConfig([FromBody] WhatsAppChannelConfig requestConfig)
        {
            var existing = await _context.WhatsAppChannelConfigs.FirstOrDefaultAsync();
            if (existing == null)
            {
                requestConfig.WhatsAppChannelConfigId = Guid.NewGuid();
                _context.WhatsAppChannelConfigs.Add(requestConfig);
            }
            else
            {
                existing.Provider = requestConfig.Provider;
                existing.BusinessPhoneNumberId = requestConfig.BusinessPhoneNumberId;
                // Only update tokens if provided
                if (!string.IsNullOrEmpty(requestConfig.EncryptedApiToken))
                    existing.EncryptedApiToken = requestConfig.EncryptedApiToken;
                if (!string.IsNullOrEmpty(requestConfig.WebhookVerifyToken))
                    existing.WebhookVerifyToken = requestConfig.WebhookVerifyToken;
                
                existing.IsEnabled = requestConfig.IsEnabled;
                existing.AutoCreateEnabled = requestConfig.AutoCreateEnabled;
                
                existing.DefaultPriorityId = requestConfig.DefaultPriorityId;
                existing.DefaultBusinessImpactId = requestConfig.DefaultBusinessImpactId;
                existing.DefaultDepartmentId = requestConfig.DefaultDepartmentId;
                existing.DefaultTicketTypeId = requestConfig.DefaultTicketTypeId;
                existing.DefaultTicketSubTypeId = requestConfig.DefaultTicketSubTypeId;
                existing.DefaultAssigneeId = requestConfig.DefaultAssigneeId;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("InboxReview")]
        public async Task<IActionResult> GetInboxReview([FromQuery] string status = "Pending")
        {
            var messages = await _context.WhatsAppInboxMessages
                .Where(m => m.ProcessingStatus == status)
                .OrderByDescending(m => m.ReceivedDate)
                .ToListAsync();
            return Ok(messages);
        }
    }
}
