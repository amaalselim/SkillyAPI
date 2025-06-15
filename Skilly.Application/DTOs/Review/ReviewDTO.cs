using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.Review
{
    public class ReviewDTO
    {
        public string providerId { get; set; }
        public string Feedback { get; set; }
        public decimal Rating { get; set; }
    }

}
