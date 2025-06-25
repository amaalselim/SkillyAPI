using Microsoft.AspNetCore.Http;
using Skilly.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.User
{
    public class edituserProfileDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Governorate { get; set; }
        public string City { get; set; }
        public string StreetName { get; set; }
        public Gender Gender { get; set; }
        public IFormFile? Img { get; set; }
    }
}
