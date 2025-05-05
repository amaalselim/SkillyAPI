using Microsoft.AspNetCore.Identity;
using Skilly.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class User :IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Please enter a valid Egyptian phone number.")]
        public string PhoneNumber {  get; set; }
        public UserType UserType { get; set; }
        public int? verificationCode { get; set; }
        //Notification
        public string? FcmToken { get; set; }
        // Location
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }



        public ICollection<Chat> ChatsOfUser { get; set; }
        public ICollection<Chat> ChatsOfProvider { get; set; }

    }
}
