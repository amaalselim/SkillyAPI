using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Skilly.Core.Entities;
using Skilly.Persistence.DataContext;
using System.Runtime.CompilerServices;

namespace Skilly.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SendMessage(string SenderId,string receiverId,string message)
        {
            var senderExists = await _context.users.AnyAsync(u => u.Id == SenderId);

            var receiverExists = await _context.users.AnyAsync(u => u.Id == receiverId);

            if (!senderExists || !receiverExists)
            {
                throw new HubException("Sender or Receiver not found.");
            }

            var msg = new Message
            {
                SenderId = SenderId,
                ReceiverId = receiverId,
                Content = message
            };

            _context.messages.Add(msg);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId).SendAsync("ReceiveMessage", SenderId, message);
        }
    }
}
