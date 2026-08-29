using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.TicketDATA;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutlookEmailController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public OutlookEmailController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: /api/OutlookEmail/Login
        [HttpGet("Login")]
        public IActionResult Login()
        {
            var clientId = _configuration["OAuthSettings:Outlook:ClientId"];
            var tenantId = _configuration["OAuthSettings:Outlook:TenantId"] ?? "common";
            var redirectUri = _configuration["OAuthSettings:Outlook:RedirectUri"];
            var scopes = "User.Read Mail.Read Mail.Send Calendars.ReadWrite offline_access";

            var oauthUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_mode=query&scope={Uri.EscapeDataString(scopes)}&state=12345";
            
            return Redirect(oauthUrl);
        }

        // GET: /api/OutlookEmail/outlook/callback
        [HttpGet("outlook/callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string error, [FromQuery] string error_description)
        {
            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest($"OAuth Error: {error} - {error_description}");
            }

            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Authorization code is missing.");
            }

            try
            {
                var clientId = _configuration["OAuthSettings:Outlook:ClientId"];
                var clientSecret = _configuration["OAuthSettings:Outlook:ClientSecret"];
                var redirectUri = _configuration["OAuthSettings:Outlook:RedirectUri"];
                var tenantId = _configuration["OAuthSettings:Outlook:TenantId"] ?? "common";

                using var client = new System.Net.Http.HttpClient();
                var tokenRequest = new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", clientId ?? "" },
                    { "scope", "User.Read Mail.Read Mail.Send Calendars.ReadWrite offline_access" },
                    { "code", code },
                    { "redirect_uri", redirectUri ?? "" },
                    { "grant_type", "authorization_code" },
                    { "client_secret", clientSecret ?? "" }
                });

                var response = await client.PostAsync($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token", tokenRequest);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return BadRequest("Failed to exchange Microsoft code: " + err);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(responseContent);
                
                var accessToken = tokenData?["access_token"]?.ToString();
                var refreshToken = tokenData?["refresh_token"]?.ToString();

                if (string.IsNullOrEmpty(accessToken))
                {
                    return BadRequest("Access token was missing in Microsoft response.");
                }

                // Fetch actual email address from Microsoft Graph API
                string authenticatedEmail = "unknown_microsoft_account@outlook.com";
                try
                {
                    using var userInfoClient = new System.Net.Http.HttpClient();
                    userInfoClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    var profileResponse = await userInfoClient.GetAsync("https://graph.microsoft.com/v1.0/me");
                    if (profileResponse.IsSuccessStatusCode)
                    {
                        var profileContent = await profileResponse.Content.ReadAsStringAsync();
                        var profileData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(profileContent);
                        
                        authenticatedEmail = profileData?["userPrincipalName"]?.ToString() 
                                             ?? profileData?["mail"]?.ToString() 
                                             ?? authenticatedEmail;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to fetch profile from Graph API: " + ex.Message);
                }

                // Query database items to set as defaults (avoid foreign key validation failures)
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
                        Provider = "Microsoft 365",
                        EmailAddress = authenticatedEmail,
                        IsEnabled = true,
                        AutoCreateEnabled = true,
                        EncryptedAccessToken = accessToken,
                        EncryptedRefreshToken = refreshToken ?? "",
                        IsAuthValid = true,
                        PollingIntervalMinutes = 15,
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
                    config.Provider = "Microsoft 365";
                    config.EncryptedAccessToken = accessToken;
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        config.EncryptedRefreshToken = refreshToken;
                    }
                    config.EmailAddress = authenticatedEmail;
                    config.IsAuthValid = true;
                }

                await _context.SaveChangesAsync();

                // Redirect back to frontend on success
                return Redirect("http://localhost:5174/admin/email-ticket-config?outlook_linked=true");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }
    }
}
