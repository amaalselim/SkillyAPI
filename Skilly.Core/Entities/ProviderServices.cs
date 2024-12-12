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
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Deliverytime { get; set; }
        public string Notes { get; set; }
        public ICollection<ProviderServicesImage> ServicesImages { get; set; } = new List<ProviderServicesImage>();
        public string serviceProviderId { get; set; }
        [JsonIgnore]
        public ServiceProvider? serviceProvider { get; set; }
        [NotMapped]
        public string serviceProviderName { get; set; }





    }
}
