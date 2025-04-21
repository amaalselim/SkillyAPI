using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class Message
    {
        public string Id { get; set; }= Guid.NewGuid().ToString();
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }= DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public virtual User Sender { get; set; }
        public virtual User Receiver { get; set; }

    }
}
