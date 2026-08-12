using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SCIQUSTICKETS.DATA.Contexts;
using SCIQUSTICKETS.DATA.DomainModels.AuthDATA;
using System.Security.Claims;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public ProfileUploadController(IWebHostEnvironment environment, UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _environment = environment;
            _userManager = userManager;
            _context = context;
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No image file provided." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var fileExt = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExt))
                return BadRequest(new { message = "Invalid image file type. Supported formats: JPG, PNG, WEBP, GIF." });

            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{userId}_{Guid.NewGuid().ToString().Substring(0, 8)}{fileExt}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarRelativeUrl = $"/uploads/avatars/{uniqueFileName}";

            // Update Employee or Account record if applicable
            var employee = await _context.Employees.FindAsync(userId);
            if (employee != null)
            {
                employee.ProfileImageUrl = avatarRelativeUrl;
                employee.LastUpdatedDate = DateTime.UtcNow;
            }

            var account = await _context.Accounts.FindAsync(userId);
            if (account != null)
            {
                account.AccountProfileImg = avatarRelativeUrl;
                account.LastUpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile picture uploaded successfully.",
                avatarUrl = avatarRelativeUrl
            });
        }
    }
}
