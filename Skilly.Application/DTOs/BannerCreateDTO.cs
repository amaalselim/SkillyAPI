using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class BannerCreateDTO
    {
        public IFormFile Image { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }
}
