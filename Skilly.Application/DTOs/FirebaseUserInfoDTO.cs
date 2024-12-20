using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class FirebaseUserInfoDTO
    {
        public string Uid { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string JwtToken { get; set; }
    }
}
