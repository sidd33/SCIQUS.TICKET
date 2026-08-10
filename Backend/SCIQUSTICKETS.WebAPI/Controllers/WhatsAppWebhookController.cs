using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWhatsAppChannelService _whatsAppService;

        public WhatsAppWebhookController(AppDbContext context, IWhatsAppChannelService whatsAppService)
        {
            _context = context;
            _whatsAppService = whatsAppService;
        }

        // Verification endpoint for webhook setup
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Verify([FromQuery(Name = "hub.mode")] string mode,
                                                [FromQuery(Name = "hub.challenge")] string challenge,
                                                [FromQuery(Name = "hub.verify_token")] string verifyToken)
        {
            var config = await _context.WhatsAppChannelConfigs.FirstOrDefaultAsync();
            if (config != null && mode == "subscribe" && verifyToken == config.WebhookVerifyToken)
            {
                return Ok(challenge);
            }
            return Unauthorized();
        }

        // Webhook receive endpoint
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Receive()
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();
            
            // Validate signature (X-Hub-Signature-256 for Meta)
            var signature = Request.Headers["X-Hub-Signature-256"].ToString();
            
            var config = await _context.WhatsAppChannelConfigs.FirstOrDefaultAsync();
            if (config == null || !_whatsAppService.ValidateWebhookSignature(payload, signature, "SECRET_PLACEHOLDER"))
            {
                return Unauthorized();
            }

            try
            {
                // Parse payload and create WhatsAppInboxMessage
                // This is a simplified placeholder parsing.
                var message = new WhatsAppInboxMessage
                {
                    WhatsAppInboxMessageId = Guid.NewGuid(),
                    ProviderMessageId = Guid.NewGuid().ToString(), // Placeholder
                    FromPhone = "+1234567890", // Placeholder
                    Body = "Webhook payload received",
                    ReceivedDate = DateTime.UtcNow
                };

                _context.WhatsAppInboxMessages.Add(message);
                await _context.SaveChangesAsync();

                // Process the message to create a ticket or thread it
                await _whatsAppService.ProcessInboxMessageAsync(message);
                await _context.SaveChangesAsync();
                
                return Ok();
            }
            catch (Exception)
            {
                // Log exception
                return StatusCode(500);
            }
        }
    }
}
