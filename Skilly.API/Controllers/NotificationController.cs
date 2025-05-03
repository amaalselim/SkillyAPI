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
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly FirebaseV1Service _firebase;

        public NotificationController(ApplicationDbContext context,FirebaseV1Service firebase)
        {
            _context = context;
            _firebase = firebase;
        }

        //[HttpPost("send-services-notification")]
        //public async Task<IActionResult> SendNotificationToRelatedCategories()
        //{
        //    var services = await _context.requestServices.ToListAsync();
        //    var providers= _context.serviceProviders
        //        .Where(c=>c.categoryId==services.)

        //}

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
            var userId = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authorized.");

            var notifications = await _context.notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(new{ notifications });
        }

        [HttpPut("mark-as-read-By/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            var notification = await _context.notifications.FindAsync(notificationId);
            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new {message="Notification Marked as read." });
        }

        [HttpDelete("Delete-Notification-By/{notificationId}")]
        public async Task<IActionResult> DeleteNotification(string notificationId)
        {
            var notification = await _context.notifications.FindAsync(notificationId);
            if (notification == null)
                return NotFound();

            _context.notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(new {messgae="Notification Deleted successfully." });
        }
    }
}
