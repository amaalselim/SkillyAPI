using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class Wallet
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [ForeignKey("provider")]
        public string ProviderId { get; set; }
        [NotMapped]
        public string? ProviderName { get; set; }
        [JsonIgnore]
        public virtual User? provider { get; set; }
        public decimal Balance { get; set; }
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public bool IsTransmitted{ get; set; } = false;


    }
}
