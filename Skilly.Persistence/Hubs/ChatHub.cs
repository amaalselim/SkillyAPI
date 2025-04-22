using global::Skilly.Core.Entities;
using Microsoft.AspNetCore.SignalR;
using Skilly.Application.DTOs;
using Skilly.Persistence.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skilly.Persistence.Hubs
{
    public class ChatHub : Hub
    {
        public static ConcurrentDictionary<string, string> Users = new ConcurrentDictionary<string, string>();
        private readonly IChatService _chatService;
        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }
        public override Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                Users[userId] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var user = Users.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(user.Key))
            {
                Users.TryRemove(user.Key, out _);
            }
            return base.OnDisconnectedAsync(exception);
        }
        public async Task GetMessages(string senderId, string receiverId)
        {
            var messages = await _chatService.GetChatAsync(senderId, receiverId);
            var messageDtos = messages.Select(m => new MessageDTO
            {
                senderId = m.SenderId,
                receiverId = m.ReceiverId,
                content = m.Content
            }).ToList();

            await Clients.Caller.SendAsync("ReceiveMessages", messageDtos);
        }
        public async Task SendMessage(string senderId, string receiverId, string content)
        {
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, content);
            await Clients.User(senderId).SendAsync("ReceiveMessage", senderId, content);

        }
    }
}
