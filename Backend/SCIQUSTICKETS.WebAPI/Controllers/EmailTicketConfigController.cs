using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using SCIQUSTICKETS.BUSINESS.Interfaces.IService;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "emailticket.config")]
    public class EmailTicketConfigController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailChannelService _emailService;
        private readonly IConfiguration _configuration;

        public EmailTicketConfigController(AppDbContext context, IEmailChannelService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public class LinkAccountRequest
        {
            public string Code { get; set; } = null!;
        }

        [HttpPost("LinkAccount")]
        public async Task<IActionResult> LinkAccount([FromBody] LinkAccountRequest request)
        {
            try
            {
                var clientId = _configuration["OAuthSettings:Google:ClientId"];
                var clientSecret = _configuration["OAuthSettings:Google:ClientSecret"];
                var redirectUri = _configuration["OAuthSettings:Google:RedirectUri"];

                using var client = new System.Net.Http.HttpClient();
                var tokenRequest = new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "code", request.Code },
                    { "client_id", clientId ?? "" },
                    { "client_secret", clientSecret ?? "" },
                    { "redirect_uri", redirectUri ?? "" },
                    { "grant_type", "authorization_code" }
                });

                var response = await client.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { error = "Failed to exchange code: " + err });
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(responseContent);
                
                var accessToken = tokenData?["access_token"]?.ToString();
                var refreshToken = tokenData?["refresh_token"]?.ToString();

                if (string.IsNullOrEmpty(accessToken))
                {
                    return BadRequest(new { error = "Access token was missing in Google response." });
                }

                // Fetch actual email address from Gmail profile endpoint (authorized by mail.google.com scope)
                string authenticatedEmail = "";
                try
                {
                    using var userInfoClient = new System.Net.Http.HttpClient();
                    userInfoClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    var profileResponse = await userInfoClient.GetAsync("https://gmail.googleapis.com/gmail/v1/users/me/profile");
                    if (profileResponse.IsSuccessStatusCode)
                    {
                        var profileContent = await profileResponse.Content.ReadAsStringAsync();
                        var profileData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(profileContent);
                        authenticatedEmail = profileData?["emailAddress"]?.ToString() ?? "";
                    }
                }
                catch (Exception ex)
                {
                    // Non-blocking log, fallback to default
                    Console.WriteLine("Failed to fetch profile from Gmail API: " + ex.Message);
                }

                // Query first active database items to set as defaults (avoid foreign key validation failures)
                var priorityObj = await _context.TicketPriorities.FirstOrDefaultAsync();
                var impactObj = await _context.TicketBusinessTypeImpacts.FirstOrDefaultAsync();
                var deptObj = await _context.Departments.FirstOrDefaultAsync();
                var typeObj = await _context.TicketTypes.FirstOrDefaultAsync();
                var subTypeObj = await _context.TicketSubTypes.FirstOrDefaultAsync();

                var config = await _context.EmailTicketConfigs.FirstOrDefaultAsync();
                if (config == null)
                {
                    config = new EmailTicketConfig
                    {
                        EmailTicketConfigId = Guid.NewGuid(),
                        Provider = "Google Workspace API (OAuth)",
                        EmailAddress = !string.IsNullOrEmpty(authenticatedEmail) ? authenticatedEmail : "siddharthaswamy01@gmail.com",
                        IsEnabled = true,
                        AutoCreateEnabled = true,
                        EncryptedAccessToken = accessToken,
                        EncryptedRefreshToken = refreshToken ?? "",
                        IsAuthValid = true,
                        DefaultPriorityId = priorityObj?.TicketPriorityId ?? Guid.Empty,
                        DefaultBusinessImpactId = impactObj?.TicketBusinessTypeImpactId ?? Guid.Empty,
                        DefaultDepartmentId = deptObj?.DepartmentId ?? Guid.Empty,
                        DefaultTicketTypeId = typeObj?.TicketTypeId ?? Guid.Empty,
                        DefaultTicketSubTypeId = subTypeObj?.TicketSubTypeId ?? Guid.Empty
                    };
                    _context.EmailTicketConfigs.Add(config);
                }
                else
                {
                    config.EncryptedAccessToken = accessToken;
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        config.EncryptedRefreshToken = refreshToken;
                    }
                    if (!string.IsNullOrEmpty(authenticatedEmail))
                    {
                        config.EmailAddress = authenticatedEmail;
                    }
                    config.IsAuthValid = true;
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Account successfully linked!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("Poll")]
        public async Task<IActionResult> PollEmails()
        {
            await _emailService.SyncMailboxesAsync();
            return Ok(new { message = "Polling process completed successfully." });
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

        public class EmailTicketConfigDto
        {
            public string Provider { get; set; } = null!;
            public string EmailAddress { get; set; } = null!;
            public int PollingIntervalMinutes { get; set; }
            public bool IsEnabled { get; set; }
            public bool AutoCreateEnabled { get; set; }
        }

        // POST: api/EmailTicketConfig
        [HttpPost]
        public async Task<IActionResult> SaveConfig([FromBody] EmailTicketConfigDto requestConfig)
        {
            var existing = await _context.EmailTicketConfigs.FirstOrDefaultAsync();
            
            // Query database items to set as defaults (avoid foreign key validation failures)
            var priorityObj = await _context.TicketPriorities.FirstOrDefaultAsync();
            var impactObj = await _context.TicketBusinessTypeImpacts.FirstOrDefaultAsync();
            var deptObj = await _context.Departments.FirstOrDefaultAsync();
            var typeObj = await _context.TicketTypes.FirstOrDefaultAsync();
            var subTypeObj = await _context.TicketSubTypes.FirstOrDefaultAsync();

            if (existing == null)
            {
                var newConfig = new EmailTicketConfig
                {
                    EmailTicketConfigId = Guid.NewGuid(),
                    Provider = requestConfig.Provider,
                    EmailAddress = requestConfig.EmailAddress,
                    IsEnabled = requestConfig.IsEnabled,
                    AutoCreateEnabled = requestConfig.AutoCreateEnabled,
                    PollingIntervalMinutes = requestConfig.PollingIntervalMinutes,
                    EncryptedAccessToken = "",
                    EncryptedRefreshToken = "",
                    IsAuthValid = false, // Not authenticated yet
                    DefaultPriorityId = priorityObj?.TicketPriorityId ?? Guid.Empty,
                    DefaultBusinessImpactId = impactObj?.TicketBusinessTypeImpactId ?? Guid.Empty,
                    DefaultDepartmentId = deptObj?.DepartmentId ?? Guid.Empty,
                    DefaultTicketTypeId = typeObj?.TicketTypeId ?? Guid.Empty,
                    DefaultTicketSubTypeId = subTypeObj?.TicketSubTypeId ?? Guid.Empty
                };
                _context.EmailTicketConfigs.Add(newConfig);
            }
            else
            {
                existing.Provider = requestConfig.Provider;
                existing.EmailAddress = requestConfig.EmailAddress;
                existing.IsEnabled = requestConfig.IsEnabled;
                existing.AutoCreateEnabled = requestConfig.AutoCreateEnabled;
                existing.PollingIntervalMinutes = requestConfig.PollingIntervalMinutes;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // GET: api/EmailTicketConfig/InboxReview
        [HttpGet("InboxReview")]
        [Authorize(Policy = "emailticket.review")]
        public async Task<IActionResult> GetInboxReview([FromQuery] string status = "All")
        {
            var query = _context.EmailInboxMessages.AsQueryable();
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(m => m.ProcessingStatus == status);
            }
            
            var messages = await query
                .OrderByDescending(m => m.EmailReceivedDate)
                .ToListAsync();
            return Ok(messages);
        }
    }
}
