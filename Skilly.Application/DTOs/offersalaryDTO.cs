using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class offersalaryDTO
    {
        public string ID { get; set; }
        public decimal Salary { get; set; }
        public string Deliverytime { get; set; }
        public string? Notes { get; set; }
        public string? serviceId { get; set; }
        public string? requestserviceId { get; set; }
        public string? ServiceName { get; set; }
        public string? RequestServiceName { get; set; }

    }
}
