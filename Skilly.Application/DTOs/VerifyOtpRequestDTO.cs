using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class VerifyOtpRequestDTO
    {
        public string PhoneNumber { get; set; }
        public string OtpCode { get; set; }
    }
}
