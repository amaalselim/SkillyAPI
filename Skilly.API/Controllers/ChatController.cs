using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.Hubs;
using System.Security.Claims;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService,IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        //[HttpPost("CreateChat")]
        //public async Task<IActionResult> CreateChat([FromBody] CreateChatDTO createChatDTO)
        //{
        //    try
        //    {
        //        var chat = await _chatService.CreateChatAsync(createChatDTO);
        //        if (chat == null)
        //        {
        //            return Ok(new { status = "success", message = "Chat already exist."});
        //        }

        //        await _hubContext.Clients.Users(createChatDTO.FirstUserId, createChatDTO.SecondUserId)
        //        .SendAsync("NewChatCreated", "A new chat has been created.");


        //        return Ok(new { status = "success", message = "Chat created successfully.", data = chat });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { status = "error", message = ex.Message });
        //    }
        //}

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDTO messageDTO)
        {
            try
            {
                var senderId= GetUserId();
                if (string.IsNullOrEmpty(senderId))
                {
                    return Unauthorized("User not authenticated");
                }

                var message = await _chatService.SendMessageAsync(messageDTO);
                if (message != null)
                {
                    if (ChatHub.Users.TryGetValue(messageDTO.receiverId, out var receiverConnectionId))
                    {
                        await _hubContext.Clients.Client(receiverConnectionId)
                            .SendAsync("ReceiveMessage", senderId, messageDTO.content);
                    }

                    if (ChatHub.Users.TryGetValue(senderId, out var senderConnectionId))
                    {
                        await _hubContext.Clients.Client(senderConnectionId)
                            .SendAsync("ReceiveMessage",senderId, messageDTO.content);
                    }

                    return Ok(new { status = "success", message = "Message sent successfully.", data = message });
                }

                return BadRequest(new { status = "error", message = "Failed to send message." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("GetChatsForUser")]
        public async Task<IActionResult> GetChatsForUser()
        {
            try
            {
                var userId= GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }
                var chats = await _chatService.GetChatsForUserAsync(userId);
                if (chats == null || !chats.Any())
                {
                    return BadRequest(new { status = "error", message = "No chats found for this user." });
                }
                await _hubContext.Clients.User(userId)
                .SendAsync("ChatsUpdated", chats);

                return Ok(new { status = "success", data = chats });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
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
                    return Unauthorized("User not authenticated");
                }
                var messages = await _chatService.GetMessagesForChatAsync(chatId, userId);
                return Ok(new { status = "success", data = messages });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("MarkChatMessagesAsRead/{chatId}")]
        public async Task<IActionResult> MarkChatMessagesAsRead(string chatId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var chat=await _chatService.MarkChatMessagesAsReadAsync(chatId, userId);


                await _hubContext.Clients.User(chat.FirstUserId).SendAsync("MessagesMarkedAsRead", chatId);
                await _hubContext.Clients.User(chat.SecondUserId).SendAsync("MessagesMarkedAsRead", chatId);


                return Ok(new { status = "success", message = "Messages marked as read." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }


    }
}
