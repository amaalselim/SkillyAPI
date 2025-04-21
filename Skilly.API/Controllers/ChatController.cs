using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Skilly.Application.DTOs;
using Skilly.Persistence.Abstract;
using System.Security.Claims;
using Skilly.Persistence.Hubs;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService; // إذا كنت تستخدم خدمة لحفظ الرسائل في قاعدة البيانات
        private readonly IHubContext<ChatHub> _hubContext; // Hub context لإرسال الرسائل عبر SignalR

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        // دالة للحصول على الـ User ID من التوكين (JWT)
        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        // إرسال رسالة إلى المستقبل عبر SignalR وحفظها في قاعدة البيانات
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDTO messageDto)
        {
            //var senderId = GetUserId();

            //if (string.IsNullOrEmpty(senderId))
            //{
            //    return Unauthorized("User not authenticated");
            //}

            // تعيين الـ senderId في الرسالة
            //messageDto.senderId = senderId;

            // حفظ الرسالة في قاعدة البيانات
            await _chatService.SendMessageAsync(messageDto);

            // إرسال الرسالة عبر SignalR إلى المستقبل
            if (await SendMessageToReceiverAsync(messageDto. senderId, messageDto.receiverId, messageDto.content))
            {
                return Ok(new { message = "Message sent successfully" });
            }
            else
            {
                return BadRequest("Failed to send message");
            }
        }

        // إرسال الرسالة إلى المستقبل عبر SignalR
        private async Task<bool> SendMessageToReceiverAsync(string senderId, string receiverId, string messageContent)
        {
            // التحقق من وجود المستقبل المتصل عبر SignalR
            if (_hubContext.Clients.User(receiverId) != null)
            {
                await _hubContext.Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, messageContent);
                return true;
            }

            return false;
        }

        // استرجاع المحادثة بين المستخدمين (المستخدم الذي أرسل الرسالة والمستقبل)
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages([FromQuery] string senderId,[FromQuery] string receiverId)
        {
            //var senderId = GetUserId();

            //if (string.IsNullOrEmpty(senderId))
            //{
            //    return Unauthorized("User not authenticated");
            //}

            // جلب الرسائل بين المرسل والمستقبل من قاعدة البيانات أو SignalR
            var messages = await _chatService.GetChatAsync(senderId, receiverId);

            return Ok(messages);
        }
    }
}

