using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class ReviewDisplayDTO
    {
        public string providerId { get; set; }
        public string userName { get; set; }
        public string userImage { get; set; }
        public string Feedback { get; set; }
        public decimal Rating { get; set; }
    }
}
