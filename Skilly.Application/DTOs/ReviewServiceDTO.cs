using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class ReviewServiceDTO
    {
        public string serviceId { get; set; }
        public string Feedback { get; set; }
        public decimal Rating { get; set; }
        [JsonIgnore]
        public string? requestserviceId { get; set; }
    }
}
