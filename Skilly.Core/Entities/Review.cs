using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class Review
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Feedback { get; set; }
        [Range(0.0, 5.0)]
        public decimal Rating { get; set; }
        [ForeignKey("ServiceProvider")]
        public string ProviderId { get; set; }
        [JsonIgnore]
        public virtual ServiceProvider? ServiceProvider { get; set; }

    }
}
