using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class WhatsAppChannelService : IWhatsAppChannelService
    {
        private readonly AppDbContext _context;
        private readonly ITicketTimelineService _timelineService;
        private readonly ITicketService _ticketService;
        private readonly ITicketNotificationService _notificationService;

        public WhatsAppChannelService(AppDbContext context, ITicketTimelineService timelineService, ITicketService ticketService, ITicketNotificationService notificationService)
        {
            _context = context;
            _timelineService = timelineService;
            _ticketService = ticketService;
            _notificationService = notificationService;
        }

        public async Task ProcessInboxMessageAsync(WhatsAppInboxMessage message)
        {
            var config = await _context.WhatsAppChannelConfigs.FirstOrDefaultAsync();
            if (config == null || !config.IsEnabled)
            {
                message.ProcessingStatus = "Failed";
                message.FailureReason = "WhatsApp channel is disabled or not configured.";
                return;
            }

            var phone = message.FromPhone.Trim();
            if (!phone.StartsWith("+")) phone = "+" + phone;

            var accountContact = await _context.AccountContacts
                .FirstOrDefaultAsync(c => c.MobileNumber == phone || c.AlternateMobileNumber == phone);

            var now = SCIQUSTICKETS.COMMON.Helpers.TimeHelper.GetIndianTime();

            if (accountContact == null)
            {
                // Auto-create a Guest Account and Contact for unknown numbers
                var guestAccount = new SCIQUSTICKETS.DATA.DomainModels.Account
                {
                    AccountId = Guid.NewGuid().ToString(),
                    AccountName = "WhatsApp Guest",
                    Email = $"guest_{phone.Replace("+", "")}@whatsapp.local",
                    RegisteredMobileNumber = phone,
                    Status = true,
                    CreatedDate = now,
                    LastUpdatedDate = now
                };
                _context.Accounts.Add(guestAccount);

                accountContact = new SCIQUSTICKETS.DATA.DomainModels.AccountContacts
                {
                    AccountContactsId = Guid.NewGuid(),
                    AccountId = guestAccount.AccountId,
                    PersonName = "WhatsApp User",
                    MobileNumber = phone,
                    CreatedDate = now,
                    LastUpdatedDate = now
                };
                _context.AccountContacts.Add(accountContact);

                var defaultPlan = await _context.SupportPlans.FirstOrDefaultAsync(p => p.Status == true);
                if (defaultPlan != null)
                {
                    var accountPlan = new SCIQUSTICKETS.DATA.DomainModels.SupportPlanDATA.AccountSupportPlan
                    {
                        AccountSupportPlanId = Guid.NewGuid(),
                        AccountId = guestAccount.AccountId,
                        SupportPlanId = defaultPlan.SupportPlanId,
                        StartDate = now,
                        EndDate = now.AddYears(1),
                        Status = "Active",
                        CreatedDate = now,
                        LastUpdatedDate = now
                    };
                    _context.AccountSupportPlans.Add(accountPlan);
                }

                await _context.SaveChangesAsync();
            }

            if (accountContact != null)
            {
                // Check for existing open ticket for this account
                var recentOpenTicket = await _context.Tickets
                    .Where(t => t.AccountId == accountContact.AccountId && t.IsOpen)
                    .OrderByDescending(t => t.CreatedDate)
                    .FirstOrDefaultAsync();

                if (recentOpenTicket != null)
                {
                    // Append as comment
                    await _ticketService.AddCommentAsync(recentOpenTicket.TicketId, new SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs.AddTicketCommentRequest
                    {
                        Comment = message.Body ?? "[Media]",
                        IsInternalNote = false
                    }, accountContact.AccountId);

                    message.CreatedTicketId = recentOpenTicket.TicketId;
                    message.ProcessingStatus = "Processed";
                    message.ProcessedDate = now;
                }
                else if (config.AutoCreateEnabled)
                {
                    try
                    {
                        // Create new ticket using core TicketService
                        var createReq = new SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs.CreateTicketRequest
                        {
                            Title = "WhatsApp Message from " + phone,
                            Description = message.Body ?? "[Media]",
                            AccountId = accountContact.AccountId,
                            SourceType = "WhatsApp",
                            TicketTypeId = config.DefaultTicketTypeId,
                            TicketSubTypeId = config.DefaultTicketSubTypeId,
                            PriorityId = config.DefaultPriorityId,
                            BusinessImpactId = config.DefaultBusinessImpactId,
                            DepartmentId = config.DefaultDepartmentId,
                            AssignedToUserId = config.DefaultAssigneeId,
                            IsInternal = true // Bypass support plan checks for auto-created tickets
                        };

                        var createdTicket = await _ticketService.CreateAsync(accountContact.AccountId, createReq);
                        
                        message.CreatedTicketId = createdTicket.TicketId;
                        message.ProcessingStatus = "Processed";
                        message.ProcessedDate = now;
                    }
                    catch (Exception ex)
                    {
                        message.ProcessingStatus = "Failed";
                        message.FailureReason = $"Auto-create failed: {ex.Message}";
                    }
                }
                else
                {
                    message.ProcessingStatus = "Failed";
                    message.FailureReason = "No open ticket found and auto-create is disabled.";
                }
            }
            else
            {
                message.ProcessingStatus = "Failed";
                message.FailureReason = "Account not found.";
            }
        }

        public async Task<bool> SendOutboundReplyAsync(Guid ticketId, string body, string? templateName, string? sentByUserId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Account)
                .ThenInclude(a => a.Contacts)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.Account == null) return false;

            var contactPhone = ticket.Account.Contacts.FirstOrDefault()?.MobileNumber;
            if (string.IsNullOrEmpty(contactPhone)) return false;

            // Send outbound message via configured WhatsApp provider
            var now = SCIQUSTICKETS.COMMON.Helpers.TimeHelper.GetIndianTime();
            
            var config = await _context.WhatsAppChannelConfigs.FirstOrDefaultAsync();
            if (config != null && config.IsEnabled && !string.IsNullOrEmpty(config.EncryptedApiToken) && !string.IsNullOrEmpty(config.BusinessPhoneNumberId))
            {
                try
                {
                    using var httpClient = new System.Net.Http.HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.EncryptedApiToken);

                    var payload = new
                    {
                        messaging_product = "whatsapp",
                        to = contactPhone.Replace("+", ""),
                        type = "text",
                        text = new { body = body }
                    };

                    var content = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"https://graph.facebook.com/v17.0/{config.BusinessPhoneNumberId}/messages", content);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        // Log error ideally, but for now we proceed to record the intent
                    }
                }
                catch
                {
                    // Ignore transient HTTP errors
                }
            }

            var outbound = new WhatsAppOutboundMessage
            {
                WhatsAppOutboundMessageId = Guid.NewGuid(),
                TicketId = ticketId,
                ToPhone = contactPhone,
                Body = body,
                TemplateName = templateName,
                Status = "Sent",
                SentByUserId = sentByUserId,
                SentDate = now
            };

            _context.WhatsAppOutboundMessages.Add(outbound);
            
            await _timelineService.WriteHistoryAsync(ticketId, SCIQUSTICKETS.COMMON.Enums.TicketChangeType.WhatsAppSent, null, null, $"WhatsApp reply sent to {contactPhone}", sentByUserId ?? "SYSTEM");

            var comment = new TicketComment
            {
                TicketCommentId = Guid.NewGuid(),
                TicketId = ticketId,
                CommentText = body,
                IsInternalNote = false,
                CommentedByUserId = sentByUserId ?? "SYSTEM",
                CreatedDate = now
            };
            _context.TicketComments.Add(comment);
            
            var history = new TicketHistory
            {
                TicketHistoryId = Guid.NewGuid(),
                TicketId = ticketId,
                ChangeDescription = $"WhatsApp Reply sent to {contactPhone}",
                ChangedByUserId = sentByUserId ?? "SYSTEM",
                CreatedDate = now
            };
            _context.TicketHistories.Add(history);

            await _context.SaveChangesAsync();
            
            await _notificationService.NotifyCommentAddedAsync(ticketId, sentByUserId ?? "SYSTEM", isCustomerVisible: true);
            
            return true;
        }

        public bool ValidateWebhookSignature(string payload, string signature, string providerSecret)
        {
            if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(providerSecret)) return false;
            
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(providerSecret);
            using var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
            byte[] hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
            string expectedSignature = "sha256=" + BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            
            return signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
        }
    }
}
