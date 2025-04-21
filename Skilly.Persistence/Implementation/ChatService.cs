using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using Skilly.Persistence.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Implementation
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatService(ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        public async Task SendMessageAsync(MessageDTO messageDTO)
        {
            var chatMessage = new Message
            {
                SenderId = messageDTO.senderId,
                ReceiverId = messageDTO.receiverId,
                Content = messageDTO.content,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();

            if (ChatHub.Users.TryGetValue(messageDTO.receiverId, out var receiverConnectionId))
            {
                await _hubContext.Clients.Client(receiverConnectionId)
                    .SendAsync("ReceiveMessage", messageDTO.senderId, messageDTO.content);
            }

            if (ChatHub.Users.TryGetValue(messageDTO.senderId, out var senderConnectionId))
            {
                await _hubContext.Clients.Client(senderConnectionId)
                    .SendAsync("ReceiveMessage", messageDTO.senderId, messageDTO.content);
            }
        }


        public async Task<List<Message>> GetChatAsync(string user1Id, string user2Id)
        {
            return await _context.Messages
                .Where(m =>
                    (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                    (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        
    }
}
