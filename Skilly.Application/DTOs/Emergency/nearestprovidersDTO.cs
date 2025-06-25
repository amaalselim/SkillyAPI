using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.Emergency
{
    public class nearestprovidersDTO
    {
        public string Id { get; set; }
        public string requestId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public double DistanceInKm { get; set; }
        public string EstimatedTimeFormatted { get; set; }
        public decimal Review{ get; set; } 
    }
}
