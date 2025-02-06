using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class requestServiceDTO
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Deliverytime { get; set; }
        public DateTime startDate { get; set; }
        public string categoryId { get; set; }
        public string? Notes { get; set; }
        public List<IFormFile>? Images { get; set; } = new List<IFormFile>();

        public string? userId { get; set; }
        
    }
}
