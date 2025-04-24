using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class servicegalleryDTO
    {
        public string galleryName { get; set; }
        public string Description { get; set; }
        public string Deliverytime { get; set; }
        public IFormFile Img { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();

    }
}
