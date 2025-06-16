using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class Payment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; }  // pending, paid, failed
        public string PaymentMethod { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("ProviderService")]
        public string? ProviderServiceId { get; set; }
        [JsonIgnore]
        public virtual ProviderServices? ProviderService { get; set; }
        public string ProviderId { get; set; }

        [ForeignKey("RequestService")]
        public string? RequestServiceId { get; set; }
        [JsonIgnore]
        public virtual RequestService? RequestService { get; set; }

        [ForeignKey("EmergencyRequest")]
        public string? EmergencyRequestId { get; set; }
        [JsonIgnore]
        public virtual EmergencyRequest? EmergencyRequest { get; set; }

        public string? PaymobOrderId { get; set; }
        public string? TransactionId { get; set; }

        public string UserId { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }
        public bool IsProcessed { get; set; }

        public string? WithdrawMethod { get; set; }
        public string? PhoneNumber { get; set; }
        public string? InstapayEmail { get; set; }
    }
}
