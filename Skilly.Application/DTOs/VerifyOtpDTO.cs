using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class VerifyOtpDTO
    {
        public string PhoneNumber { get; set; }
        public int otpCode { get; set; }
    }
}
