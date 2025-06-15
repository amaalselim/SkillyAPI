using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.Review
{
    public class ReviewDisplayDTO
    {
        public string providerId { get; set; }
        public string serviceName { get; set; }
        public string userName { get; set; }
        public string userImage { get; set; }
        public string Feedback { get; set; }
        public decimal Rating { get; set; }
        //public decimal AvgRate { get; set; }
    }
}
