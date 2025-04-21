using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class MessageDTO
    {
        public string senderId { get; set; }
        public string receiverId {  get; set; } 
        public string content { get; set; }
    }
}
