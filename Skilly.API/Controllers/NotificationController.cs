using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
using Skilly.Persistence.DataContext;
using System.Security.Claims;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly FirebaseV1Service _firebase;

        public NotificationController(ApplicationDbContext context, FirebaseV1Service firebase)
        {
            _context = context;
            _firebase = firebase;
        }

        private string GetUserIdFromClaims()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authorized.");
            }
            return userId;
        }

        [HttpGet("GetUserNotifications")]
        public async Task<IActionResult> GetUserNotifications()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { status = "error", message = "User not authorized." });

                var notifications = await _context.notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                return Ok(new { status = "success", notifications });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPut("mark-as-read-By/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            try
            {
                var notification = await _context.notifications.FindAsync(notificationId);
                if (notification == null)
                    return NotFound(new { status = "error", message = "Notification not found." });

                notification.IsRead = true;
                await _context.SaveChangesAsync();

                return Ok(new { status = "success", message = "Notification marked as read." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpDelete("Delete-Notification-By/{notificationId}")]
        public async Task<IActionResult> DeleteNotification(string notificationId)
        {
            try
            {
                var notification = await _context.notifications.FindAsync(notificationId);
                if (notification == null)
                    return NotFound(new { status = "error", message = "Notification not found." });

                _context.notifications.Remove(notification);
                await _context.SaveChangesAsync();

                return Ok(new { status = "success", message = "Notification deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}
