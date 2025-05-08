using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class ProviderservicesDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Deliverytime { get; set; }
        public string? Notes { get; set; }
        public decimal Price { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public string categoryId { get; set; }
    }
}
