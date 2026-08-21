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
    [Authorize(Policy = "whatsapp.config")]
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
                config.EncryptedApiToken,
                config.WebhookVerifyToken,
                config.AppSecret,
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

        public class WhatsAppConfigDto
        {
            public WhatsAppProviderType Provider { get; set; }
            public string? BusinessPhoneNumberId { get; set; }
            public string? EncryptedApiToken { get; set; }
            public string? WebhookVerifyToken { get; set; }
            public string? AppSecret { get; set; }
            
            public bool? IsEnabled { get; set; }
            public bool? AutoCreateEnabled { get; set; }
            
            public Guid? DefaultPriorityId { get; set; }
            public Guid? DefaultBusinessImpactId { get; set; }
            public Guid? DefaultDepartmentId { get; set; }
            public Guid? DefaultTicketTypeId { get; set; }
            public Guid? DefaultTicketSubTypeId { get; set; }
            public string? DefaultAssigneeId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SaveConfig([FromBody] WhatsAppConfigDto requestConfig)
        {
            var existing = await _context.WhatsAppChannelConfigs.FirstOrDefaultAsync();
            if (existing == null)
            {
                var newConfig = new WhatsAppChannelConfig
                {
                    WhatsAppChannelConfigId = Guid.NewGuid(),
                    Provider = requestConfig.Provider,
                    BusinessPhoneNumberId = requestConfig.BusinessPhoneNumberId ?? "",
                    EncryptedApiToken = requestConfig.EncryptedApiToken ?? "",
                    WebhookVerifyToken = requestConfig.WebhookVerifyToken ?? "",
                    AppSecret = requestConfig.AppSecret ?? "",
                    IsEnabled = requestConfig.IsEnabled ?? false,
                    AutoCreateEnabled = requestConfig.AutoCreateEnabled ?? false,
                    DefaultPriorityId = requestConfig.DefaultPriorityId ?? Guid.Empty,
                    DefaultBusinessImpactId = requestConfig.DefaultBusinessImpactId ?? Guid.Empty,
                    DefaultDepartmentId = requestConfig.DefaultDepartmentId ?? Guid.Empty,
                    DefaultTicketTypeId = requestConfig.DefaultTicketTypeId ?? Guid.Empty,
                    DefaultTicketSubTypeId = requestConfig.DefaultTicketSubTypeId ?? Guid.Empty,
                    DefaultAssigneeId = requestConfig.DefaultAssigneeId
                };
                _context.WhatsAppChannelConfigs.Add(newConfig);
            }
            else
            {
                existing.Provider = requestConfig.Provider;
                if (!string.IsNullOrEmpty(requestConfig.BusinessPhoneNumberId))
                    existing.BusinessPhoneNumberId = requestConfig.BusinessPhoneNumberId;
                // Only update tokens if provided
                if (!string.IsNullOrEmpty(requestConfig.EncryptedApiToken))
                    existing.EncryptedApiToken = requestConfig.EncryptedApiToken;
                if (!string.IsNullOrEmpty(requestConfig.WebhookVerifyToken))
                    existing.WebhookVerifyToken = requestConfig.WebhookVerifyToken;
                if (!string.IsNullOrEmpty(requestConfig.AppSecret))
                    existing.AppSecret = requestConfig.AppSecret;
                
                if (requestConfig.IsEnabled.HasValue)
                    existing.IsEnabled = requestConfig.IsEnabled.Value;
                if (requestConfig.AutoCreateEnabled.HasValue)
                    existing.AutoCreateEnabled = requestConfig.AutoCreateEnabled.Value;
                
                if (requestConfig.DefaultPriorityId.HasValue && requestConfig.DefaultPriorityId.Value != Guid.Empty)
                    existing.DefaultPriorityId = requestConfig.DefaultPriorityId.Value;
                if (requestConfig.DefaultBusinessImpactId.HasValue && requestConfig.DefaultBusinessImpactId.Value != Guid.Empty)
                    existing.DefaultBusinessImpactId = requestConfig.DefaultBusinessImpactId.Value;
                if (requestConfig.DefaultDepartmentId.HasValue && requestConfig.DefaultDepartmentId.Value != Guid.Empty)
                    existing.DefaultDepartmentId = requestConfig.DefaultDepartmentId.Value;
                if (requestConfig.DefaultTicketTypeId.HasValue && requestConfig.DefaultTicketTypeId.Value != Guid.Empty)
                    existing.DefaultTicketTypeId = requestConfig.DefaultTicketTypeId.Value;
                if (requestConfig.DefaultTicketSubTypeId.HasValue && requestConfig.DefaultTicketSubTypeId.Value != Guid.Empty)
                    existing.DefaultTicketSubTypeId = requestConfig.DefaultTicketSubTypeId.Value;
                if (!string.IsNullOrEmpty(requestConfig.DefaultAssigneeId))
                    existing.DefaultAssigneeId = requestConfig.DefaultAssigneeId;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("InboxReview")]
        [Authorize(Policy = "whatsapp.review")]
        public async Task<IActionResult> GetInboxReview([FromQuery] string? status = null)
        {
            var query = _context.WhatsAppInboxMessages.AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(m => m.ProcessingStatus == status);
            }
            var messages = await query.OrderByDescending(m => m.ReceivedDate).ToListAsync();
            return Ok(messages);
        }
    }
}
