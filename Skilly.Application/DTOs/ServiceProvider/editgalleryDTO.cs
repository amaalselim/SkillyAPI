using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.ServiceProvider
{
    public class editgalleryDTO
    {
        public string galleryName { get; set; }
        public string Description { get; set; }
        public string Deliverytime { get; set; }

        public List<string>? ImagesToDeleteIds { get; set; }
        public List<IFormFile>? Images { get; set; }
        public IFormFile? video { get; set; }
    }
}
