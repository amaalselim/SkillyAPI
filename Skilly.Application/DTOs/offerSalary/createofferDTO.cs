using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class createofferDTO
    {
        public decimal Salary { get; set; }
        public string Deliverytime { get; set; }
        public string? Notes { get; set; }
        public string? serviceId { get; set; }
        [JsonIgnore]
        public string? requestserviceId { get; set; }
    }
}
