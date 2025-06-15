using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.ServiceProvider
{
    public class EditProviderServiceDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Deliverytime { get; set; }
        public string? Notes { get; set; }
        public decimal Price { get; set; }
        public List<string>? ImagesToDeleteIds { get; set; }
        public List<IFormFile>? Images { get; set; }
        public IFormFile? video { get; set; }
    }



}
