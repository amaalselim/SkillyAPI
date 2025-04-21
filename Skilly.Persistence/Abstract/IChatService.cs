using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IChatService
    {
        Task SendMessageAsync(MessageDTO messageDTO);
        Task<List<Message>> GetChatAsync(string user1Id, string user2Id);

        //Task<List<MessageResponseDto>> GetMessagesBetweenUsersAsync(string userId1, string userId2);
        //Task<List<ChatResponseDto>> GetChatsForUserAsync(string userId);
        //Task<bool> MarkAsReadAsync(string messageId);
        //Task<bool> EditMessageAsync(string messageId, string newContent);
        //Task<bool> DeleteMessageAsync(string messageId);
        //Task<Message?> GetMessageByIdAsync(string messageId);

    }
}
