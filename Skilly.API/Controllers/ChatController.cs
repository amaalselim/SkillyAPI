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
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDTO messageDto)
        {
            try
            {
                //var senderId = GetUserId();

                //    //if (string.IsNullOrEmpty(senderId))
                //    //{
                //    //    return Unauthorized("User not authenticated");
                //    //}

                //    //messageDto.senderId = senderId;
                await _chatService.SendMessageAsync(messageDto);

                await _hubContext.Clients.User(messageDto.receiverId)
                    .SendAsync("ReceiveMessage", messageDto.senderId, messageDto.content);

                await _hubContext.Clients.User(messageDto.senderId)
                    .SendAsync("ReceiveMessage", messageDto.senderId, messageDto.content);

                return Ok(new { message = "Message sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }



        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages([FromQuery] string senderId,[FromQuery] string receiverId)
        {
            //var senderId = GetUserId();

            //if (string.IsNullOrEmpty(senderId))
            //{
            //    return Unauthorized("User not authenticated");
            //}
            var messages = await _chatService.GetChatAsync(senderId, receiverId);

            return Ok(messages);
        }
    }
}

