using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class ProviderServices
    {
        public string Id { get; set; }= Guid.NewGuid().ToString();
        public DateOnly? ServiceRequestTime { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Deliverytime { get; set; }
        public string? Notes { get; set; }
        [JsonIgnore]
        public ICollection<ProviderServicesImage> ServicesImages { get; set; } = new List<ProviderServicesImage>();
        [ForeignKey("serviceProvider")]
        public string serviceProviderId { get; set; }
        [NotMapped]
        public string ServiceProviderName { get; set; }
        public string providerImg { get; set; }
        [JsonIgnore]
        public string uId { get; set; }
        [JsonIgnore]
        public ServiceProvider? serviceProvider { get; set; }
        [NotMapped]
        public List<string> Images { get; set; }

        [ForeignKey("Category")]
        public string categoryId { get; set; }
        [JsonIgnore]    
        public virtual Category? Category { get; set; }
        [JsonIgnore]
        public ICollection<OfferSalary>? offerSalaries { get; set; } = new List<OfferSalary>();
    }
}
