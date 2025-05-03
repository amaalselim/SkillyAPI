using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class Notifications
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("User")]
        public string UserId { get; set; }
        [JsonIgnore]
        public virtual User? User { get; set; }
        
        public string Title { get; set; }
        public string Body { get; set; }  
        public bool IsRead { get; set; } = false; 
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}
