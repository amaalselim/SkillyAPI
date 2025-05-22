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
        Task<ChatDTO> CreateChatAsync(CreateChatDTO dto);
        Task<Message> SendMessageAsync(MessageDTO dto);
        Task<List<ChatDTO>> GetChatsForUserAsync(string userId);
        Task<List<MessageResponseDto>> GetMessagesForChatAsync(string chatId, string userId);
        Task<string> MarkChatMessagesAsReadAsync(string messageId, string userId);
    }
}
