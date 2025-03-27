using Microsoft.AspNetCore.SignalR;
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
