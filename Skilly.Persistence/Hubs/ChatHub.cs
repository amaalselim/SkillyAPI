using Microsoft.AspNetCore.SignalR;
using Skilly.Application.DTOs;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Skilly.Persistence.Hubs
{
    public class ChatHub : Hub
    {
        // قاموس لتخزين الاتصال (Connection ID) الخاص بكل مستخدم
        public static ConcurrentDictionary<string, string> Users = new ConcurrentDictionary<string, string>();

        public ChatHub()
        {
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

        public async Task NotifyNewChat(string firstUserId, string secondUserId)
        {
            await Clients.Users(firstUserId, secondUserId)
                .SendAsync("NewChatCreated", "A new chat has been created.");
        }

        public async Task NotifyChatExists(string firstUserId, string secondUserId)
        {
            await Clients.Users(firstUserId, secondUserId)
                .SendAsync("ChatExists", "The chat already exists.");
        }

        public async Task NotifyMessageReceived(string senderId, string receiverId, string content)
        {
            if (Users.TryGetValue(receiverId, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId)
                    .SendAsync("ReceiveMessage", senderId, content);
            }

            if (Users.TryGetValue(senderId, out var senderConnectionId))
            {
                await Clients.Client(senderConnectionId)
                    .SendAsync("ReceiveMessage", senderId, content);
            }
        }

        public async Task NotifyChatsUpdated(string userId)
        {
            await Clients.User(userId)
                .SendAsync("ChatsUpdated", "Your chat list has been updated.");
        }

        public async Task NotifyMessagesUpdated(string chatId)
        {
            await Clients.Group(chatId)
                .SendAsync("MessagesUpdated", "Messages have been updated.");
        }
    }
}
