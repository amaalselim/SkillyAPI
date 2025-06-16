using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.Payment
{
    public class WithdrawRequestDTO
    {
        [JsonIgnore]
        public string? ProviderId { get; set; }
        [JsonIgnore]
        public decimal? Amount { get; set; }
        public string WithdrawMethod { get; set; }
        public string? PhoneNumber { get; set; }
        public string? InstapayEmail { get; set; }
    }
}
