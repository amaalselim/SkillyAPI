using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class requestServiceImage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Img { get; set; }
        [ForeignKey("RequestService")]
        public string requestServiceId { get; set; }
        [JsonIgnore]
        public virtual RequestService? RequestService { get; set; }
    }
}
