using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class LoginDTO
    {
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "يرجى إدخال رقم هاتف مصري صحيح.")]
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }

    }
}
