using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.Emergency
{
    public class EmergencyOfferDTO
    {
        public string RequestId { get; set; }
        public string ProviderId { get; set; }
    }
}
