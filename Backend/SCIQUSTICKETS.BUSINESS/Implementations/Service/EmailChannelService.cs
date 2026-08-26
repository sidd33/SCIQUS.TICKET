using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;

namespace SCIQUSTICKETS.BUSINESS.Implementations.Service
{
    public class EmailChannelService : IEmailChannelService
    {
        private readonly AppDbContext _context;
        private readonly ITicketTimelineService _timelineService;
        private readonly ITicketService _ticketService;
        private readonly ISlaService _slaService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailChannelService> _logger;

        public EmailChannelService(
            AppDbContext context,
            ITicketTimelineService timelineService,
            ITicketService ticketService,
            ISlaService slaService,
            IConfiguration configuration,
            ILogger<EmailChannelService> logger)
        {
            _context = context;
            _timelineService = timelineService;
            _ticketService = ticketService;
            _slaService = slaService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ProcessInboxMessageAsync(EmailInboxMessage message)
        {
            var config = await _context.EmailTicketConfigs.FirstOrDefaultAsync();
            if (config == null || !config.IsEnabled)
            {
                message.ProcessingStatus = "Failed";
                message.FailureReason = "Email channel is disabled or not configured.";
                return;
            }

            // Attempt to find account by sender email
            var accountContact = await _context.AccountContacts
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.Email == message.FromEmail);

            var now = SCIQUSTICKETS.COMMON.Helpers.TimeHelper.GetIndianTime();

            if (accountContact == null)
            {
                // Auto-create a Guest Account and Contact for unknown email address
                var guestAccount = new SCIQUSTICKETS.DATA.DomainModels.Account
                {
                    AccountId = Guid.NewGuid().ToString(),
                    AccountName = "Email Guest",
                    Email = message.FromEmail,
                    RegisteredMobileNumber = "N/A",
                    Status = true,
                    CreatedDate = now,
                    LastUpdatedDate = now
                };
                _context.Accounts.Add(guestAccount);

                accountContact = new SCIQUSTICKETS.DATA.DomainModels.AccountContacts
                {
                    AccountContactsId = Guid.NewGuid(),
                    AccountId = guestAccount.AccountId,
                    PersonName = string.IsNullOrEmpty(message.FromName) ? "Email User" : message.FromName,
                    MobileNumber = null,
                    Email = message.FromEmail,
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
                Ticket? existingTicket = null;
                
                // Attempt to thread by extracting TKT-\d+ from subject
                var match = Regex.Match(message.Subject, @"TKT-\d+");
                if (match.Success)
                {
                    string ticketNum = match.Value;
                    existingTicket = await _context.Tickets
                        .FirstOrDefaultAsync(t => t.TicketNumber == ticketNum && t.AccountId == accountContact.AccountId && t.IsOpen);
                }

                if (existingTicket != null)
                {
                    // Thread as comment
                    await _ticketService.AddCommentAsync(existingTicket.TicketId, new SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs.AddTicketCommentRequest
                    {
                        Comment = message.Body,
                        IsInternalNote = false
                    }, accountContact.AccountId);

                    message.CreatedTicketId = existingTicket.TicketId;
                    message.ProcessingStatus = "Processed";
                    message.ProcessedDate = now;
                }
                else if (config.AutoCreateEnabled)
                {
                    try
                    {
                        var createReq = new SCIQUSTICKETS.BUSINESS.BusinessModels.RequestDTOs.TicketRequestDTOs.CreateTicketRequest
                        {
                            Title = message.Subject,
                            Description = message.Body,
                            AccountId = accountContact.AccountId,
                            SourceType = "Email",
                            TicketTypeId = config.DefaultTicketTypeId,
                            TicketSubTypeId = config.DefaultTicketSubTypeId,
                            PriorityId = config.DefaultPriorityId,
                            BusinessImpactId = config.DefaultBusinessImpactId,
                            DepartmentId = config.DefaultDepartmentId,
                            AssignedToUserId = config.DefaultAssigneeId,
                            IsInternal = false
                        };

                        var createdTicket = await _ticketService.CreateAsync(accountContact.AccountId, createReq);
                        
                        // Fix SLA clock for email tickets
                        var ticketEntity = await _context.Tickets.FindAsync(createdTicket.TicketId);
                        if (ticketEntity != null)
                        {
                            ticketEntity.EmailReceivedDate = message.EmailReceivedDate;
                            var priority = await _context.TicketPriorities.FindAsync(ticketEntity.PriorityId);
                            var (slaDueDate, slaMetStatus) = _slaService.ComputeSlaDueDate(ticketEntity, priority);
                            ticketEntity.SlaDueDate = slaDueDate;
                            ticketEntity.SlaMetStatus = slaMetStatus;
                        }
                        
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

        public async Task<bool> SendOutboundReplyAsync(Guid ticketId, string body, string? sentByUserId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Account)
                .ThenInclude(a => a.Contacts)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null || ticket.Account == null) return false;

            // Find primary contact email
            var contactEmail = ticket.Account.Contacts.FirstOrDefault()?.Email;
            if (string.IsNullOrEmpty(contactEmail)) return false;

            // Send outbound email via configured provider (SMTP)
            var config = await _context.EmailTicketConfigs.FirstOrDefaultAsync();
            if (config != null && config.IsEnabled && !string.IsNullOrEmpty(config.AppPassword))
            {
                try
                {
                    string smtpHost = config.Provider == "Outlook" ? "smtp.office365.com" : "smtp.gmail.com";
                    int smtpPort = 587; // STARTTLS

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Support", config.EmailAddress));
                    message.To.Add(new MailboxAddress(ticket.Account.AccountName, contactEmail));
                    message.Subject = $"Re: [TKT-{ticket.TicketNumber}] {ticket.Title}";
                    
                    message.Body = new TextPart("plain")
                    {
                        Text = body
                    };

                    using var client = new MailKit.Net.Smtp.SmtpClient();
                    await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(config.EmailAddress, config.AppPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send SMTP email to {Email}", contactEmail);
                }
            }

            var now = SCIQUSTICKETS.COMMON.Helpers.TimeHelper.GetIndianTime();
            
            // Add comment to ticket
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
            
            await _timelineService.WriteHistoryAsync(ticketId, SCIQUSTICKETS.COMMON.Enums.TicketChangeType.EmailSent, null, null, $"Email reply sent to {contactEmail}", sentByUserId ?? "SYSTEM");
            
            var history = new TicketHistory
            {
                TicketHistoryId = Guid.NewGuid(),
                TicketId = ticketId,
                ChangeDescription = $"Email Reply sent to {contactEmail}",
                ChangedByUserId = sentByUserId ?? "SYSTEM",
                CreatedDate = now
            };
            _context.TicketHistories.Add(history);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SyncMailboxesAsync()
        {
            var config = await _context.EmailTicketConfigs.FirstOrDefaultAsync();
            if (config == null || !config.IsEnabled) return;

            string emailAddress = config.EmailAddress;
            string accessToken = config.EncryptedAccessToken;

            // Refresh token helper to ensure access token is valid
            if (!string.IsNullOrEmpty(config.EncryptedRefreshToken) && (config.Provider == "Google" || config.Provider.Contains("OAuth") || config.Provider.Contains("Google Workspace API")))
            {
                try
                {
                    var clientId = _configuration["OAuthSettings:Google:ClientId"];
                    var clientSecret = _configuration["OAuthSettings:Google:ClientSecret"];

                    using var httpClient = new System.Net.Http.HttpClient();
                    var tokenRequest = new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "client_id", clientId ?? "" },
                        { "client_secret", clientSecret ?? "" },
                        { "refresh_token", config.EncryptedRefreshToken },
                        { "grant_type", "refresh_token" }
                    });

                    _logger.LogInformation("[Gmail Poller] Refreshing access token...");
                    var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var tokenData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(responseContent);
                        var newAccessToken = tokenData?["access_token"]?.ToString();
                        if (!string.IsNullOrEmpty(newAccessToken))
                        {
                            config.EncryptedAccessToken = newAccessToken;
                            await _context.SaveChangesAsync();
                            accessToken = newAccessToken;
                            _logger.LogInformation("[Gmail Poller] Access token successfully refreshed!");
                        }
                    }
                    else
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        _logger.LogError("[Gmail Poller] Failed to refresh access token: {Error}", err);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Gmail Poller] Exception occurred while refreshing token");
                }
            }

            if (config.Provider == "Google Workspace API (OAuth)" || config.Provider == "Google" || config.Provider.Contains("Google"))
            {
                // ── Google REST API Polling (No IMAP) ──
                if (!string.IsNullOrEmpty(accessToken))
                {
                    try
                    {
                        using var httpClient = new System.Net.Http.HttpClient();
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                        _logger.LogInformation("[Gmail Poller] Sync started via Gmail REST API...");
                        var listResponse = await httpClient.GetAsync("https://gmail.googleapis.com/gmail/v1/users/me/messages?q=is:unread");
                        if (!listResponse.IsSuccessStatusCode)
                        {
                            var err = await listResponse.Content.ReadAsStringAsync();
                            _logger.LogError("Failed to list Gmail messages: {Error}", err);
                            config.LastError = $"Failed to list messages: {err}";
                            await _context.SaveChangesAsync();
                            return;
                        }

                        var listContent = await listResponse.Content.ReadAsStringAsync();
                        var listData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(listContent);
                        var messagesArray = listData?["messages"]?.AsArray();

                        _logger.LogInformation("[Gmail Poller] found {Count} unseen messages.", messagesArray?.Count ?? 0);

                        if (messagesArray != null && messagesArray.Count > 0)
                        {
                            var existingIds = new HashSet<string>(
                                await _context.EmailInboxMessages
                                    .Select(m => m.ProviderMessageId)
                                    .ToListAsync()
                            );

                            foreach (var msgNode in messagesArray)
                            {
                                var msgId = msgNode?["id"]?.ToString();
                                if (string.IsNullOrEmpty(msgId) || existingIds.Contains(msgId)) continue;

                                // Fetch individual message details
                                var detailResponse = await httpClient.GetAsync($"https://gmail.googleapis.com/gmail/v1/users/me/messages/{msgId}");
                                if (!detailResponse.IsSuccessStatusCode) continue;

                                var detailContent = await detailResponse.Content.ReadAsStringAsync();
                                var msgData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(detailContent);

                                // Parse headers
                                var headers = msgData?["payload"]?["headers"]?.AsArray();
                                string fromHeader = "";
                                string subjectHeader = "";
                                string dateHeader = "";

                                if (headers != null)
                                {
                                    foreach (var header in headers)
                                    {
                                        var name = header?["name"]?.ToString()?.ToLower();
                                        var value = header?["value"]?.ToString() ?? "";
                                        if (name == "from") fromHeader = value;
                                        else if (name == "subject") subjectHeader = value;
                                        else if (name == "date") dateHeader = value;
                                    }
                                }

                                // Parse body from payload parts
                                var payload = msgData?["payload"];
                                string body = ExtractTextBody(payload);

                                // Parse From Email and Name
                                string fromEmail = "unknown@unknown.com";
                                string fromName = "";
                                if (!string.IsNullOrEmpty(fromHeader))
                                {
                                    var emailMatch = Regex.Match(fromHeader, @"<([^>]+)>");
                                    if (emailMatch.Success)
                                    {
                                        fromEmail = emailMatch.Groups[1].Value.Trim();
                                        fromName = fromHeader.Replace(emailMatch.Value, "").Replace("\"", "").Trim();
                                    }
                                    else
                                    {
                                        fromEmail = fromHeader.Trim();
                                    }
                                }

                                // Parse date
                                DateTime receivedDate = DateTime.UtcNow;
                                if (DateTime.TryParse(dateHeader, out var parsedDate))
                                {
                                    receivedDate = parsedDate.ToUniversalTime();
                                }

                                var inboxMsg = new EmailInboxMessage
                                {
                                    EmailInboxMessageId = Guid.NewGuid(),
                                    ProviderMessageId = msgId,
                                    FromEmail = fromEmail,
                                    FromName = string.IsNullOrEmpty(fromName) ? null : fromName,
                                    Subject = string.IsNullOrEmpty(subjectHeader) ? "(No Subject)" : subjectHeader,
                                    Body = body,
                                    EmailReceivedDate = receivedDate,
                                    ProcessingStatus = "Pending",
                                    CreatedDate = DateTime.UtcNow
                                };

                                _context.EmailInboxMessages.Add(inboxMsg);

                                // Mark email as read in Gmail (remove UNREAD label)
                                var modifyRequestObj = new
                                {
                                    removeLabelIds = new[] { "UNREAD" }
                                };
                                var modifyContent = new System.Net.Http.StringContent(
                                    System.Text.Json.JsonSerializer.Serialize(modifyRequestObj),
                                    System.Text.Encoding.UTF8,
                                    "application/json"
                                );
                                var modifyResponse = await httpClient.PostAsync($"https://gmail.googleapis.com/gmail/v1/users/me/messages/{msgId}/modify", modifyContent);
                                if (modifyResponse.IsSuccessStatusCode)
                                {
                                    _logger.LogInformation("[Gmail Poller] Marked Gmail message {MsgId} as read.", msgId);
                                }
                            }

                            await _context.SaveChangesAsync();
                        }

                        // Update last poll date
                        config.LastPollDate = DateTime.UtcNow;
                        config.LastError = null;
                        config.IsAuthValid = true;
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Gmail REST API sync error for {EmailAddress}", emailAddress);
                        config.LastError = ex.Message;
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    _logger.LogWarning("Google Workspace API token missing — skipping Gmail REST API fetch.");
                }
            }
            else
            {
                // ── Fallback IMAP Polling (using MailKit) ──
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    try
                    {
                        string imapHost = config.Provider == "Outlook" ? "outlook.office365.com" : (config.EmailAddress.Contains("gmail") ? "imap.gmail.com" : "outlook.office365.com");
                        int imapPort = 993;
                        bool useSsl = true;

                        using var client = new ImapClient();
                        await client.ConnectAsync(imapHost, imapPort, useSsl);
                        
                        if (config.Provider == "IMAP")
                        {
                            if (!string.IsNullOrEmpty(config.AppPassword))
                            {
                                await client.AuthenticateAsync(emailAddress, config.AppPassword);
                            }
                            else
                            {
                                _logger.LogWarning("IMAP provider selected but no App Password provided. Skipping.");
                                return;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(accessToken))
                            {
                                _logger.LogWarning("OAuth provider selected but no access token found. Skipping.");
                                return;
                            }
                            var oauth2 = new MailKit.Security.SaslMechanismOAuth2(emailAddress, accessToken);
                            await client.AuthenticateAsync(oauth2);
                        }

                        var inbox = client.Inbox;
                        await inbox.OpenAsync(FolderAccess.ReadWrite);

                        var unseenUids = await inbox.SearchAsync(SearchQuery.NotSeen);
                        _logger.LogInformation("IMAP sync: found {Count} unseen messages.", unseenUids.Count);

                        if (unseenUids.Count > 0)
                        {
                            var messages = await inbox.FetchAsync(unseenUids, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.Body);

                            var existingIds = new HashSet<string>(
                                await _context.EmailInboxMessages
                                    .Select(m => m.ProviderMessageId)
                                    .ToListAsync()
                            );

                            var uidsToMarkSeen = new List<UniqueId>();

                            foreach (var summary in messages)
                            {
                                var uid = summary.UniqueId.ToString();

                                if (existingIds.Contains(uid))
                                {
                                    _logger.LogDebug("Skipping already-processed message UID: {Uid}", uid);
                                    uidsToMarkSeen.Add(summary.UniqueId);
                                    continue;
                                }

                                var mime = await inbox.GetMessageAsync(summary.UniqueId);

                                var fromAddress = mime.From.Mailboxes.FirstOrDefault();
                                var subject = mime.Subject ?? "(No Subject)";
                                var body = mime.TextBody ?? mime.HtmlBody ?? string.Empty;
                                if (string.IsNullOrEmpty(mime.TextBody) && !string.IsNullOrEmpty(mime.HtmlBody))
                                {
                                    body = System.Text.RegularExpressions.Regex.Replace(mime.HtmlBody, "<.*?>", " ").Trim();
                                }

                                var inboxMsg = new EmailInboxMessage
                                {
                                    EmailInboxMessageId = Guid.NewGuid(),
                                    ProviderMessageId = uid,
                                    FromEmail = fromAddress?.Address ?? "unknown@unknown.com",
                                    FromName = fromAddress?.Name,
                                    Subject = subject,
                                    Body = body,
                                    EmailReceivedDate = mime.Date.UtcDateTime,
                                    ProcessingStatus = "Pending",
                                    CreatedDate = DateTime.UtcNow
                                };

                                _context.EmailInboxMessages.Add(inboxMsg);
                                uidsToMarkSeen.Add(summary.UniqueId);
                            }

                            await _context.SaveChangesAsync();

                            if (uidsToMarkSeen.Count > 0)
                            {
                                await inbox.AddFlagsAsync(uidsToMarkSeen, MessageFlags.Seen, true);
                            }
                        }

                        await client.DisconnectAsync(true);

                        config.LastPollDate = DateTime.UtcNow;
                        config.LastError = null;
                        config.IsAuthValid = true;
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "IMAP sync error for {EmailAddress}", emailAddress);
                        config.LastError = ex.Message;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // ── Step 2: Process all Pending messages (real + mock) into tickets ──
            await ProcessPendingDbMessagesAsync();
        }

        private async Task ProcessPendingDbMessagesAsync()
        {
            var pendingMessages = await _context.EmailInboxMessages
                .Where(m => m.ProcessingStatus == "Pending")
                .ToListAsync();

            foreach (var message in pendingMessages)
            {
                await ProcessInboxMessageAsync(message);
            }

            if (pendingMessages.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        private string ExtractTextBody(System.Text.Json.Nodes.JsonNode? node)
        {
            if (node == null) return "";

            var mimeType = node["mimeType"]?.ToString() ?? "";
            var bodyDataNode = node["body"]?["data"];
            
            if (bodyDataNode != null && (mimeType.Contains("text/plain") || mimeType.Contains("text/html")))
            {
                var base64Url = bodyDataNode.ToString();
                try
                {
                    string base64 = base64Url.Replace('-', '+').Replace('_', '/');
                    switch (base64.Length % 4)
                    {
                        case 2: base64 += "=="; break;
                        case 3: base64 += "="; break;
                    }
                    byte[] bytes = Convert.FromBase64String(base64);
                    string decoded = System.Text.Encoding.UTF8.GetString(bytes);

                    if (mimeType.Contains("text/html"))
                    {
                        decoded = System.Text.RegularExpressions.Regex.Replace(decoded, "<.*?>", " ").Trim();
                    }
                    return decoded;
                }
                catch
                {
                    return "";
                }
            }

            var parts = node["parts"]?.AsArray();
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    var partMime = part?["mimeType"]?.ToString() ?? "";
                    if (partMime.Contains("text/plain"))
                    {
                        var text = ExtractTextBody(part);
                        if (!string.IsNullOrEmpty(text)) return text;
                    }
                }
                foreach (var part in parts)
                {
                    var partMime = part?["mimeType"]?.ToString() ?? "";
                    if (partMime.Contains("text/html"))
                    {
                        var text = ExtractTextBody(part);
                        if (!string.IsNullOrEmpty(text)) return text;
                    }
                }
                foreach (var part in parts)
                {
                    var text = ExtractTextBody(part);
                    if (!string.IsNullOrEmpty(text)) return text;
                }
            }

            return "";
        }
    }
}
