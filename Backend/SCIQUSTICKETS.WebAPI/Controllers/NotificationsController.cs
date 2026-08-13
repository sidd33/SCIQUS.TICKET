using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIQUSTICKETS.DATA.Contexts;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace SCIQUSTICKETS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false, [FromQuery] int limit = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var query = _context.NotificationUsers
                .Include(nu => nu.Notification)
                .Where(nu => nu.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(nu => !nu.IsRead);
            }

            var notifications = await query
                .OrderByDescending(nu => nu.Notification.CreatedDate)
                .Take(limit)
                .Select(nu => new
                {
                    nu.NotificationId,
                    nu.IsRead,
                    nu.Notification.EventType,
                    nu.Notification.RedirectUrl,
                    nu.Notification.CreatedDate,
                    Message = nu.Notification.EventType + " update on " + nu.Notification.EntityType
                })
                .ToListAsync();

            return Ok(new { items = notifications });
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var notificationUser = await _context.NotificationUsers
                .FirstOrDefaultAsync(nu => nu.NotificationId == id && nu.UserId == userId);

            if (notificationUser == null) return NotFound();

            notificationUser.IsRead = true;
            notificationUser.ReadDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
