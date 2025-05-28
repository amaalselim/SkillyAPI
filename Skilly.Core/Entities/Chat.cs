using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class Chat
    {
        public string Id { get; set; }= Guid.NewGuid().ToString();

        public string FirstUserId { get; set; }
        [NotMapped]
        public string FirstUserName { get; set; }
        public virtual User FirstUser { get; set; }

        public string SecondUserId { get; set; }
        [NotMapped]
        public string SecondUserName { get; set; }
        [NotMapped]
        public string SecondUserImg { get; set; }
        public virtual User SecondUser { get; set; }

        public ICollection<Message> Messages { get; set; }
        [NotMapped]
        public string? LastMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
