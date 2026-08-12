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
            // Note: Meta signs payloads using the App Secret, not the Verify Token. 
            // Bypassing signature validation temporarily for testing until an App Secret field is added to the config.
            if (config == null)
            {
                return Unauthorized();
            }

            try
            {
                using var jsonDoc = JsonDocument.Parse(payload);
                var root = jsonDoc.RootElement;
                if (root.TryGetProperty("entry", out var entryArray) && entryArray.GetArrayLength() > 0)
                {
                    var entry = entryArray[0];
                    if (entry.TryGetProperty("changes", out var changesArray) && changesArray.GetArrayLength() > 0)
                    {
                        var value = changesArray[0].GetProperty("value");
                        if (value.TryGetProperty("messages", out var messagesArray) && messagesArray.GetArrayLength() > 0)
                        {
                            var messageData = messagesArray[0];
                            string fromPhone = messageData.GetProperty("from").GetString() ?? "";
                            string providerMsgId = messageData.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
                            string msgType = messageData.GetProperty("type").GetString() ?? "text";
                            
                            string body = "";
                            if (msgType == "text" && messageData.TryGetProperty("text", out var textNode))
                            {
                                body = textNode.GetProperty("body").GetString() ?? "";
                            }
                            else
                            {
                                body = $"[Received media type: {msgType}]";
                            }

                            string fromName = "";
                            if (value.TryGetProperty("contacts", out var contactsArray) && contactsArray.GetArrayLength() > 0)
                            {
                                var contact = contactsArray[0];
                                if (contact.TryGetProperty("profile", out var profile))
                                {
                                    fromName = profile.GetProperty("name").GetString() ?? "";
                                }
                            }

                            var message = new WhatsAppInboxMessage
                            {
                                WhatsAppInboxMessageId = Guid.NewGuid(),
                                ProviderMessageId = providerMsgId,
                                FromPhone = fromPhone,
                                FromName = fromName,
                                MessageType = msgType,
                                Body = body,
                                ReceivedDate = DateTime.UtcNow
                            };

                            _context.WhatsAppInboxMessages.Add(message);
                            await _context.SaveChangesAsync();

                            await _whatsAppService.ProcessInboxMessageAsync(message);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                // Log exception
                return StatusCode(500, ex.Message);
            }
        }

        // Temporary endpoint so you can see your messages in the browser
        [HttpGet("debug-inbox")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugInbox()
        {
            var messages = await _context.WhatsAppInboxMessages.OrderByDescending(m => m.ReceivedDate).ToListAsync();
            return Ok(messages);
        }
    }
}
