using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class ChatDTO
    {
        public string Id { get; set; }
        public string FirstUserId { get; set; }
        public string FirstUserName { get; set; }

        public string SecondUserId { get; set; }
        public string SecondUserName { get; set; }
        public string? firstUserImg {  get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        public string? SecondUserImg { get; set; }
        public string? lastMessage { get; set; } 
    }

}
