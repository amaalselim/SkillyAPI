using Microsoft.AspNetCore.Http;
using Skilly.Application.Abstract;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.Implementation
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task DeleteFileAsync(string File)
        {
            if (File != null)
            {
                var RootPath = _webHostEnvironment.WebRootPath;
                var oldFile = Path.Combine(RootPath, File);

                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Delete(oldFile);
                }
            }
        }
        public async Task<string> SaveFileAsync(IFormFile file, string folderPath)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            string extension = Path.GetExtension(file.FileName).ToLower();
            string fullFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, folderPath);

            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".mp4", ".mov", ".avi", ".mkv", ".m4v" };

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only image, video, or PDF files are allowed.");
            }

            string filePath = Path.Combine(fullFolderPath, file.FileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }


            return Path.Combine(folderPath, file.FileName); 
        }




    }
}
