using Skilly.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class RegisterDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Please enter a valid Egyptian phone number.")]
        public string PhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email  { get; set; }
        public string Password { get; set; }

        public UserType UserType { get; set; }
        //public string FcmToken { get; set; }
    }
}
