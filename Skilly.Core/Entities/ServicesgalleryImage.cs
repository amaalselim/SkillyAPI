using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class ServicesgalleryImage
    {
        [JsonIgnore]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Img {  get; set; }
        [JsonIgnore]
        [ForeignKey("Servicesgallery")]

        public string galleryId { get; set; }

        [JsonIgnore]
        public Servicesgallery? Servicesgallery { get; set; }
    }
}
