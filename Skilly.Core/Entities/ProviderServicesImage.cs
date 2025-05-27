using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class ProviderServicesImage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Img { get; set; }
        [JsonIgnore]
        [ForeignKey("ProviderServices")]
        public string serviceId { get; set; }
        [JsonIgnore]
        public ProviderServices? ProviderServices { get; set; }
    }
}
