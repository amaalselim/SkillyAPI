using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class ChatRoom
    {
        public string ChatRoomId { get; set; } 
        public ICollection<string> UserIds { get; set; } 
        public ICollection<Message> Messages { get; set; }
    }
}
