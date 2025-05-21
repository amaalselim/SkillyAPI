using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
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
        public virtual ProviderServices? ProviderService { get; set; }

        [ForeignKey("RequestService")]
        public string? RequestServiceId { get; set; }
        public virtual RequestService? RequestService { get; set; }

        public string? PaymobOrderId { get; set; }
        public string? TransactionId { get; set; }

        public string UserId { get; set; }

        public virtual User User { get; set; }
    }
}
