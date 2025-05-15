using Skilly.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class OfferSalary
    {
        public string Id { get; set; }= Guid.NewGuid().ToString();
        public decimal Salary { get; set; }
        public string Deliverytime { get; set; }
        public string? Notes { get; set; }
        [ForeignKey(nameof(ProviderServices))]
        public string? serviceId { get; set; }
        [JsonIgnore]
        public virtual ProviderServices? ProviderServices { get; set; }
        [ForeignKey(nameof(RequestService))]
        public string? requestserviceId { get; set; }
        [JsonIgnore]
        public virtual RequestService? RequestService { get; set; }
        [ForeignKey(nameof(User))]
        public string? userId { get; set; }
        [JsonIgnore]
        public virtual User? User { get; set; }
        public OfferStatus Status { get; set; } = OfferStatus.Pending;

    }
}
