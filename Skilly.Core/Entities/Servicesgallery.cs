using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class Servicesgallery
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string galleryName { get; set; }
        public string? Description { get; set; }
        public string Deliverytime { get; set; }
        
        [JsonIgnore]
        public ICollection<ServicesgalleryImage> galleryImages { get; set; } = new List<ServicesgalleryImage>();
        [ForeignKey("serviceProvider")]
        public string serviceProviderId { get; set; }
        [JsonIgnore]
        public ServiceProvider? serviceProvider { get; set; }
        [NotMapped]
        public List<string> Images { get; set; }
        public string? video{ get; set; }

    }
}
