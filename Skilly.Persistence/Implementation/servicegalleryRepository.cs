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
            if (servicegalleryDTO.video != null)
            {
                if(servicegalleryDTO.video.ContentType != "video/mp4")
                {
                    throw new InvalidOperationException("Invalid file type. Only mp4 files are allowed.");
                }
                gallery.video= await _imageService.SaveFileAsync(servicegalleryDTO.video, path);
            }
            if (servicegalleryDTO.Images != null && servicegalleryDTO.Images.Any())
            {
                foreach (var image in servicegalleryDTO.Images)
                {
                    var imagePath = await _imageService.SaveFileAsync(image, path);

                    gallery.galleryImages.Add(new ServicesgalleryImage
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
                .Include(g => g.galleryImages)
                .FirstOrDefaultAsync(g => g.Id == galleryId && g.serviceProviderId == user.Id);

            if (gallery == null)
            {
                throw new ServiceGalleryNotFoundException("Gallery not found.");
            }
            _context.servicesgalleries.Remove(gallery);
            await _context.SaveChangesAsync();
        }

        public async Task EditServiceGallery(servicegalleryDTO servicegalleryDTO, string userId, string galleryId)
        {
            var gallery = await _context.servicesgalleries
            .Include(g => g.galleryImages)
            .FirstOrDefaultAsync(g => g.Id == galleryId && g.serviceProviderId == userId);

            if (gallery == null)
            {
                throw new ServiceGalleryNotFoundException("Gallery not found.");
            }
            _mapper.Map(servicegalleryDTO, gallery);
            var path = @"Images/ServiceProvider/Servicegallery/";
            if (servicegalleryDTO.video != null)
            {
                if (servicegalleryDTO.video.ContentType != "video/mp4")
                {
                    throw new InvalidOperationException("Invalid file type. Only mp4 files are allowed.");
                }
                gallery.video = await _imageService.SaveFileAsync(servicegalleryDTO.video, path);
            }
            if (servicegalleryDTO.Images != null && servicegalleryDTO.Images.Any())
            {
                

                if (servicegalleryDTO.Images != null && servicegalleryDTO.Images.Any())
                {

                    foreach (var image in servicegalleryDTO.Images)
                    {
                        var imagePath = await _imageService.SaveFileAsync(image, path);
                        gallery.galleryImages.Add(new ServicesgalleryImage
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
                .Include(i => i.galleryImages)
                .Include(i => i.serviceProvider)
                .ToListAsync();

            if (gallery == null || !gallery.Any())
            {
                return new List<Servicesgallery>();
            }

            var galleryDtos = gallery.Select(item => new Servicesgallery
            {
                Id = item.Id,
                galleryName= item.galleryName,
                Description = item.Description,
                Deliverytime = item.Deliverytime,
                
                serviceProviderId = item.serviceProviderId,
                Images = item.galleryImages?
                    .Select(img => img.Img)
                    .ToList() ?? new List<string>(),
                video = item.video,
            }).ToList();

            return galleryDtos;
        }

        public async Task<Servicesgallery> GetServiceGalleryByIdAsync(string galleryId, string userId)
        {
            var provider = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            var gallery = await _context.servicesgalleries
                .Include(i => i.galleryImages)
                .Include(g => g.serviceProvider)
                .FirstOrDefaultAsync(g => g.Id == galleryId && g.serviceProviderId == provider.Id);

            if (gallery == null)
            {
                throw new ServiceGalleryNotFoundException("Gallery not found.");
            }

            var galleryDto = new Servicesgallery
            {
                Id = gallery.Id,
                galleryName = gallery.galleryName,
                Description = gallery.Description,
                Deliverytime = gallery.Deliverytime,
                serviceProviderId = gallery.serviceProviderId,
                Images = gallery.galleryImages?
                    .Select(img => img.Img)
                    .ToList() ?? new List<string>(),
                video = gallery.video,
            };

            return galleryDto;
        }

        public async Task<IEnumerable<Servicesgallery>> GetAllgalleryByPProviderId(string providerId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.Id== providerId);
            var service = await _context.servicesgalleries
                .Include(i => i.galleryImages)
                .Where(c => c.serviceProviderId == user.Id)
                .ToListAsync();

            if (service == null || !service.Any())
            {
                return new List<Servicesgallery>();
            }

            var galleryDtos = service.Select(item => new Servicesgallery
            {
                Id = item.Id,
                galleryName = item.galleryName,
                Description = item.Description,
                Deliverytime = item.Deliverytime,
                serviceProviderId = item.serviceProviderId,
                Images = item.galleryImages?
                    .Select(img => img.Img)
                    .ToList() ?? new List<string>(),
                video = item.video,
            }).ToList();

            return galleryDtos;
        }

        public async Task<IEnumerable<Servicesgallery>> GetAllgalleryByProviderId(string providerId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == providerId);
            var service = await _context.servicesgalleries
                .Include(i => i.galleryImages)
                .Where(c => c.serviceProviderId == user.Id)
                .ToListAsync();

            if (service == null || !service.Any())
            {
                return new List<Servicesgallery>();
            }

            var galleryDtos = service.Select(item => new Servicesgallery
            {
                Id = item.Id,
                galleryName = item.galleryName,
                Description = item.Description,
                Deliverytime = item.Deliverytime,
                serviceProviderId = item.serviceProviderId,
                Images = item.galleryImages?
                    .Select(img => img.Img)
                    .ToList() ?? new List<string>(),
                video = item.video,
            }).ToList();

            return galleryDtos;
        }
    }
}
