using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Implementation
{
    public class servicegalleryRepository : IServicegalleryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public servicegalleryRepository(ApplicationDbContext context,IMapper mapper,IImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task AddServiceGallery(servicegalleryDTO servicegalleryDTO, string userId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId ==userId);
            if (user == null)
            {
                throw new ServiceProviderNotFoundException("Service Provider not found.");
            }
            var path = @"Images/ServiceProvider/Servicegallery/";
            var gallery = _mapper.Map<Servicesgallery>(servicegalleryDTO);
            gallery.serviceProviderId = user.Id;
            gallery.serviceProviderName=user.FirstName+" "+user.LastName;
            if (servicegalleryDTO.Img != null)
            {
                gallery.Img = await _imageService.SaveFileAsync(servicegalleryDTO.Img, path);
            }
            if (servicegalleryDTO.Images != null && servicegalleryDTO.Images.Any())
            {
                foreach (var image in servicegalleryDTO.Images)
                {
                    var imagePath = await _imageService.SaveFileAsync(image, path);

                    gallery.Images.Add(new ServicesgalleryImage
                    {
                        Img = imagePath,
                        galleryId = gallery.Id
                    });
                }
            }

            await _context.servicesgalleries.AddAsync(gallery);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteServiceGalleryAsync(string galleryId, string userId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            var gallery = await _context.servicesgalleries
                .Include(g => g.Images)
                .FirstOrDefaultAsync(g => g.Id == galleryId && g.serviceProviderId == user.Id);

            if (gallery == null)
            {
                throw new ServiceGalleryNotFoundException("Gallery not found.");
            }
            var path = @"Images/ServiceProvider/Servicegallery/";
            if (!string.IsNullOrEmpty(gallery.Img))
            {
                string imagePath = Path.Combine(path, gallery.Img);
                await _imageService.DeleteFileAsync(imagePath);
            }

            foreach (var image in gallery.Images)
            {
                string imagePath = Path.Combine(path, gallery.Img);
                await _imageService.DeleteFileAsync(imagePath);
            }

            gallery.Images.Clear();
            _context.servicesgalleries.Remove(gallery);
            await _context.SaveChangesAsync();
        }

        public async Task EditServiceGallery(servicegalleryDTO servicegalleryDTO, string userId, string galleryId)
        {
            var gallery = await _context.servicesgalleries
            .Include(g => g.Images)
            .FirstOrDefaultAsync(g => g.Id == galleryId && g.serviceProviderId == userId);

            if (gallery == null)
            {
                throw new ServiceGalleryNotFoundException("Gallery not found.");
            }
            _mapper.Map(servicegalleryDTO, gallery);
            var path = @"Images/ServiceProvider/Servicegallery/";

            if (servicegalleryDTO.Img != null)
            {
                if (!string.IsNullOrEmpty(gallery.Img))
                {
                    string imagePath = Path.Combine(path, gallery.Img);
                    await _imageService.DeleteFileAsync(gallery.Img);
                }
                
                gallery.Img = await _imageService.SaveFileAsync(servicegalleryDTO.Img, path);
            }
            if (servicegalleryDTO.Images != null && servicegalleryDTO.Images.Any())
            {
                foreach (var image in gallery.Images)
                {
                    await _imageService.DeleteFileAsync(image.Img);
                }
                gallery.Images.Clear();

                if (servicegalleryDTO.Images != null && servicegalleryDTO.Images.Any())
                {

                    foreach (var image in servicegalleryDTO.Images)
                    {
                        var imagePath = await _imageService.SaveFileAsync(image, path);
                        gallery.Images.Add(new ServicesgalleryImage
                        {
                            Img = imagePath,
                            galleryId = gallery.Id
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Servicesgallery>> GetAllServiceGallery()
        {
            var gallery = await _context.servicesgalleries
                .Include(i => i.Images) 
                .Include(i => i.serviceProvider) 
                .ToListAsync();

            if (gallery == null || !gallery.Any())
            {
                return new List<Servicesgallery>(); 
            }
            foreach (var item in gallery)
            {
                item.serviceProviderName = item.serviceProvider != null ? item.serviceProvider.FirstName + " " + item.serviceProvider.LastName : "NUll";
                item.Images = item.Images.Where(img => img.Img.StartsWith("Images/ServiceProvider/Servicegallery/")).ToList();

            }

            return gallery;
        }


        public async Task<Servicesgallery> GetServiceGalleryByIdAsync(string galleryId, string userId)
        {
            var provider = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            var gallery = await _context.servicesgalleries
                .Include(g => g.Images) 
                .Include(g=>g.serviceProvider)
                .FirstOrDefaultAsync(g => g.Id == galleryId && g.serviceProviderId == provider.Id);

            if (gallery == null)
            {
                throw new ServiceGalleryNotFoundException("Gallery not found.");
            }
            gallery.Images = gallery.Images.Where(img => img.Img.StartsWith("Images/ServiceProvider/Servicegallery/")).ToList();
            return gallery;
        }

    }
}
