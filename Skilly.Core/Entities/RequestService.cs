using Skilly.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class RequestService
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateOnly? ServiceRequestTime { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string Name {  get; set; }
        public decimal Price { get; set; }
        public string Deliverytime { get; set; }
        public DateOnly? startDate { get; set; }
        [ForeignKey("Category")]
        public string categoryId { get; set; }
        [JsonIgnore]
        public virtual Category? Category { get; set; }

        public string? Notes { get; set; }
        [ForeignKey("UserProfile")]
        public string userId { get; set; }
        [JsonIgnore]
        public string uId { get; set; }
        [NotMapped]
        public string userName { get; set; }
        public string userImg { get; set; }
        [NotMapped]
        public List<string> Images { get; set; }
        [JsonIgnore]
        public virtual UserProfile? UserProfile { get; set; }
        [JsonIgnore]
        public ICollection<requestServiceImage>? requestServiceImages{ get; set; } = new List<requestServiceImage>();
        [JsonIgnore]
        public ICollection<OfferSalary>? offerSalaries{ get; set; } = new List<OfferSalary>();
        [NotMapped]
        public int OffersCount { get; set; }

        [NotMapped]
        public double Distance { get; set; }

        [JsonIgnore]
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [JsonIgnore]
        public ServiceStatus ServiceStatus { get; set; } = ServiceStatus.Posted;
        [JsonIgnore]
        public string? providerId {  get; set; }
        [JsonIgnore]
        public virtual ServiceProvider? ServiceProvider { get; set; }

    }
}
