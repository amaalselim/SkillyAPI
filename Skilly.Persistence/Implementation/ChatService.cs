using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using Skilly.Persistence.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Implementation
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatService(ApplicationDbContext context, IHubContext<ChatHub> hubContext,IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _hubContext = hubContext;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ChatDTO> CreateChatAsync(CreateChatDTO dto)
        {
            var existingChat = await _context.chats
                .Where(c => (c.FirstUserId == dto.FirstUserId && c.SecondUserId == dto.SecondUserId) ||
                (c.FirstUserId == dto.SecondUserId && c.SecondUserId == dto.FirstUserId))
                .FirstOrDefaultAsync();
            if (existingChat != null)
            {
                return null;
            }

            var chat=_mapper.Map<Chat>(dto);

            chat.CreatedAt=DateTime.Now;
            chat.LastUpdatedAt = DateTime.Now;

            _context.chats.Add(chat);
            await _context.SaveChangesAsync();
            

            await _hubContext.Clients.Users(dto.FirstUserId, dto.SecondUserId)
            .SendAsync("NewChatCreated", "A new chat has been created.");


            return _mapper.Map<ChatDTO>(chat);
        }
        public string GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId;
        }


        public async Task<MessageDTO> SendMessageAsync(MessageDTO dto)
        {
            var senderId = GetUserId();

            var chat = await _context.chats
                .FirstOrDefaultAsync(c =>
                    (c.FirstUserId == senderId && c.SecondUserId == dto.receiverId) ||
                    (c.FirstUserId == dto.receiverId && c.SecondUserId == senderId));

            if (chat == null)
            {
                var createChatDto = new CreateChatDTO
                {
                    FirstUserId = senderId,
                    SecondUserId = dto.receiverId
                };

                var createdChat = await CreateChatAsync(createChatDto);

                if (createdChat == null)
                {
                    return null;
                }
                chat = _mapper.Map<Chat>(createdChat); 
            }

            var message = new Message
            {
                ChatId = chat.Id,
                SenderId = senderId,
                ReceiverId = dto.receiverId,
                Content = dto.content,
                SentAt = DateTime.Now,
                Timestamp = DateTime.Now,
                IsRead = false
            };


            _context.Messages.Add(message);
            await _context.SaveChangesAsync();


            chat.LastUpdatedAt = DateTime.Now;
            _context.chats.Update(chat);
            await _context.SaveChangesAsync();

            if (ChatHub.Users.TryGetValue(dto.receiverId, out var receiverConnectionId))
            {
                await _hubContext.Clients.Client(receiverConnectionId)
                    .SendAsync("ReceiveMessage", senderId, dto.content);
            }

            if (ChatHub.Users.TryGetValue(senderId, out var senderConnectionId))
            {
                await _hubContext.Clients.Client(senderConnectionId)
                    .SendAsync("ReceiveMessage", senderId, dto.content);
            }

            return _mapper.Map<MessageDTO>(message);
        }
        public async Task<List<ChatDTO>> GetChatsForUserAsync(string userId)
        {
            var chats = await _context.chats
                .Where(c => c.FirstUserId == userId || c.SecondUserId == userId)
                .Include(c => c.FirstUser)
                .Include(c => c.SecondUser)
                .OrderByDescending(c => c.LastUpdatedAt)
                .ToListAsync();

            var chatDtos = _mapper.Map<List<ChatDTO>>(chats);

            foreach (var chatDto in chatDtos)
            {
                var chat = chats.FirstOrDefault(c => c.Id == chatDto.Id);
                if (chat != null)
                {
                    chatDto.FirstUserName = chat.FirstUser.FirstName + " " + chat.FirstUser.LastName;
                    chatDto.SecondUserName = chat.SecondUser.FirstName + " " + chat.SecondUser.LastName;
                }
            }

            if (chatDtos.Count > 0)
            {
                await _hubContext.Clients.User(userId)
                    .SendAsync("ChatsUpdated", chatDtos);
            }

            return chatDtos;
        }
        public async Task<List<MessageResponseDto>> GetMessagesForChatAsync(string chatId, string userId)
        {
            var chat = await _context.chats
                .Where(c => c.Id == chatId && (c.FirstUserId == userId || c.SecondUserId == userId))
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                throw new Exception("The user is not part of this chat.");
            }

            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .Include(m => m.Sender)   
                .Include(m => m.Receiver) 
                .ToListAsync();

            var messageDtos = _mapper.Map<List<MessageResponseDto>>(messages);

            for (int i = 0; i < messageDtos.Count; i++)
            {
                var message = messages[i];
                messageDtos[i].SenderName = message.Sender.FirstName + " " + message.Sender.LastName;
                messageDtos[i].ReceiverName = message.Receiver.FirstName + " " + message.Receiver.LastName;
            }

            await _hubContext.Clients.Users(chat.FirstUserId, chat.SecondUserId)
                .SendAsync("MessagesUpdated", messageDtos);

            return messageDtos;
        }

        public async Task<Chat> MarkChatMessagesAsReadAsync(string chatId, string userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId && m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();

            if (!messages.Any())
            {
                return null; 
            }

            foreach (var message in messages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            var chat = await _context.chats
                .Where(c => c.Id == chatId)
                .FirstOrDefaultAsync();

            return chat;
        }


    }
}
