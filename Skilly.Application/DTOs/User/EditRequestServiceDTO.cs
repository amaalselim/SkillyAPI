using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.User
{
    public class EditRequestServiceDTO
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Deliverytime { get; set; }
        public DateOnly startDate { get; set; }
        public string categoryId { get; set; }
        public string? Notes { get; set; }
        public List<IFormFile>? Images { get; set; } = new List<IFormFile>();
        public List<string>? ImagesToDeleteIds { get; set; }
        public IFormFile? video { get; set; }
    }
}
