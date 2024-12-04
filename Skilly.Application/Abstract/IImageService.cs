using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.Abstract
{
    public interface IImageService
    {
        Task<string> SaveFileAsync(IFormFile Img, string folderPath);
        Task DeleteFileAsync(string File);
    }
}
