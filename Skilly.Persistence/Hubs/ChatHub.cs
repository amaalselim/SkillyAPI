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
        
        // تخزين الاتصالات بناءً على userId
        public static ConcurrentDictionary<string, string> Users = new ConcurrentDictionary<string, string>();
        private readonly IChatService _chatService;
        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }


        // عند الاتصال
        public override Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                Users[userId] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        // عند الانفصال
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


        // إرسال الرسالة
        public async Task SendMessage(string senderId, string receiverId, string messageContent)
        {
            var messageDTO = new MessageDTO
            {
                senderId = senderId,
                receiverId = receiverId,
                content = messageContent
            };

            // نادينا السيرفس بالـ DTO
            await _chatService.SendMessageAsync(messageDTO);
            // إرسال للطرف المستقبل (لو متصل)
            //if (Users.TryGetValue(receiverId, out var receiverConnectionId))
            //{
            //    await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderId, messageContent);
            //}

            //// إرسال لنفس المرسل
            //if (Users.TryGetValue(senderId, out var senderConnectionId))
            //{
            //    await Clients.Client(senderConnectionId).SendAsync("ReceiveMessage", senderId, messageContent);
            //}
        }

        // جلب الرسائل بين المرسل والمستقبل
        //public async Task GetMessages(string senderId, string receiverId)
        //{
        //    var messages = await GetMessagesFromDatabase(senderId, receiverId);
        //    await Clients.Caller.SendAsync("ReceiveMessages", messages);
        //}

        // محاكاة لجلب الرسائل
        private Task<List<Message>> GetMessagesFromDatabase(string senderId, string receiverId)
        {
            var mockMessages = new List<Message>
            {
                new Message { SenderId = senderId, ReceiverId = receiverId, Content = "Hello!", Timestamp = DateTime.UtcNow.AddMinutes(-10) },
                new Message { SenderId = receiverId, ReceiverId = senderId, Content = "Hi, how are you?", Timestamp = DateTime.UtcNow.AddMinutes(-5) }
            };

            return Task.FromResult(mockMessages);
        }
    }
}
