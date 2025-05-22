using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class MessageResponseDto
    {
        public string Id { get; set; }
        public string ChatId { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }

        public string ReceiverId { get; set; }
        public string ReceiverName { get; set; }

        public DateTime SentAt { get; set; }
        public string? Img { get; set; }
    }
}
