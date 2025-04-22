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
        public DateTime ServiceRequestTime { get; set; } = DateTime.UtcNow;
        public string Name {  get; set; }
        public decimal Price { get; set; }
        public string Deliverytime { get; set; }
        public DateTime startDate { get; set; }
        [ForeignKey("Category")]
        public string categoryId { get; set; }
        [JsonIgnore]
        public virtual Category? Category { get; set; }
        public string? Notes { get; set; }
        [ForeignKey("UserProfile")]
        public string userId { get; set; }
        [NotMapped]
        public string userName { get; set; }
        [JsonIgnore]
        public virtual UserProfile? UserProfile { get; set; }
        public ICollection<requestServiceImage>? requestServiceImages{ get; set; } = new List<requestServiceImage>();
        public ICollection<OfferSalary>? offerSalaries{ get; set; } = new List<OfferSalary>();

    }
}
