using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class ReviewDTO
    {
        public string ProviderId { get; set; }
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string Feedback { get; set; }
        public decimal Rating { get; set; }
    }
}
