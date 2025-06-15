using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Skilly.Application.DTOs;
using Skilly.Application.DTOs.chat;
using Skilly.Application.Exceptions;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.Migrations;
using System.Security.Claims;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromForm] MessageDTO messageDTO)
        {
            try
            {
                var senderId = GetUserId();
                if (string.IsNullOrEmpty(senderId))
                {
                    return Unauthorized(new { status = "error", message = "User not authenticated" });
                }

                if (string.IsNullOrWhiteSpace(messageDTO.content) && messageDTO.Img == null)
                {
                    return BadRequest(new { status = "error", message = "Please provide a message or an image." });
                }

                var message = await _chatService.SendMessageAsync(messageDTO);
                if (message != null)
                {
                    await _hubContext.Clients.User(messageDTO.receiverId)
                        .SendAsync("ReceiveMessage", senderId, message.Content, message.Img);

                    await _hubContext.Clients.User(senderId)
                        .SendAsync("ReceiveMessage", senderId, message.Content, message.Img);

                    return StatusCode(201, new
                    {
                        status = "success",
                        message = "Message sent successfully.",
                        data = new
                        {
                            SenderId=senderId,
                            ReceiverId = message.ReceiverId,
                            Content = message.Content,
                            Image = message.Img,
                        }
                    });
                }

                return BadRequest(new { status = "error", message = "Failed to send message." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message, details = ex.InnerException?.Message });
            }

        }
        [HttpGet("GetChatsForUser")]
        public async Task<IActionResult> GetChatsForUser()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { status = "error", message = "User not authenticated" });
                }
                var chats = await _chatService.GetChatsForUserAsync(userId);
                if (chats == null || !chats.Any())
                {
                    return NotFound(new { status = "error", message = "No chats found for this user." });
                }
                await _hubContext.Clients.User(userId)
                    .SendAsync("ChatsUpdated", chats);

                return Ok(new { status = "success", data = chats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("GetMessagesForChatOfUser/{chatId}")]
        public async Task<IActionResult> GetMessagesForChat(string chatId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { status = "error", message = "User not authenticated" });
                }
                var messages = await _chatService.GetMessagesForChatAsync(chatId, userId);

                await _hubContext.Clients.User(userId)
                    .SendAsync("MessagesForChatUpdated", messages);

                return Ok(new { status = "success", data = messages });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("MarkMessageAsRead")]
        public async Task<IActionResult> MarkMessageAsRead([FromBody] MarksasReadDTO marksasReadDTO)
        {
            if (string.IsNullOrEmpty(marksasReadDTO.MessageId))
            {
                return BadRequest(new { status = "error", message = "Message ID is required." });
            }

            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { status = "error", message = "User not authenticated" });
                }

                var message = await _chatService.MarkChatMessagesAsReadAsync(marksasReadDTO.MessageId, userId);

                if (message != null)
                {
                    await _hubContext.Clients.User(userId)
                        .SendAsync("MessageRead", marksasReadDTO.MessageId);

                    return Ok(new { status = "success", message = "Message marked as read." });
                }

                return BadRequest(new { status = "error", message = "Failed to mark message as read." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}
