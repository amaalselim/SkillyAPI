using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class CategoryDTO
    {
        public string Name { get; set; }
        public IFormFile Img { get; set; }
    }
}
