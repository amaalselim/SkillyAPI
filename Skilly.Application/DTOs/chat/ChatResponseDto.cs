using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class ChatResponseDto
    {
        public string User1Id { get; set; }
        public string User2Id { get; set; }
        public List<MessageResponseDto> Messages { get; set; }
    }
}
