using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Skilly.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        public decimal? PriceDiscount { get; set; }
        public string Deliverytime { get; set; }
        public string? Notes { get; set; }
       
        [JsonIgnore]
        public ICollection<ProviderServicesImage>? ServicesImages { get; set; } = new List<ProviderServicesImage>();
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
        public List<ProviderServicesImage> Images { get; set; }

        public string? video { get; set; }

        [ForeignKey("Category")]
        public string categoryId { get; set; }
        [JsonIgnore]    
        public virtual Category? Category { get; set; }
        [JsonIgnore]
        public ICollection<OfferSalary>? offerSalaries { get; set; } = new List<OfferSalary>();
        [NotMapped]
        public int CountOfOffers { get; set; }
        [NotMapped]
        public double Distance { get; set; }
        [JsonIgnore]
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        [JsonIgnore]
        public ServiceStatus ServiceStatus { get; set; } = ServiceStatus.Posted;
        [JsonIgnore]
        public string? userprofileId { get; set; }
        [JsonIgnore]
        public virtual User? UserProfile { get; set; }
        public ICollection<Review>? Reviews { get; set; } = new List<Review>();


    }
}
