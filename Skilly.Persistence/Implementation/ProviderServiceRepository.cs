using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using Skilly.Persistence.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Implementation
{
    public class ProviderServiceRepository : IProviderServicesRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public ProviderServiceRepository(ApplicationDbContext context,IMapper mapper,IImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }
        public async Task AddProviderService(ProviderservicesDTO providerservicesDTO, string userId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ServiceProviderNotFoundException("Service Provider not found.");
            }
            var path = @"Images/ServiceProvider/MyServices/";
            var service = _mapper.Map<ProviderServices>(providerservicesDTO);
            service.serviceProviderId = user.Id;
            
            if (providerservicesDTO.Images != null && providerservicesDTO.Images.Any())
            {
                foreach (var image in providerservicesDTO.Images)
                {
                    var imagePath = await _imageService.SaveFileAsync(image, path);

                    service.ServicesImages.Add(new ProviderServicesImage
                    {
                        Img = imagePath,
                        serviceId = service.Id
                    });
                }
            }

            await _context.providerServices.AddAsync(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProviderServiceAsync(string serviceId, string userId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            var service = await _context.providerServices
            .Include(g => g.ServicesImages)
                .FirstOrDefaultAsync(g => g.Id == serviceId && g.serviceProviderId == user.Id);

            if (service == null)
            {
                throw new ProviderServiceNotFoundException("Service not found.");
            }
            var path = @"Images/ServiceProvider/MyServices/";


            service.ServicesImages.Clear();
            _context.providerServices.Remove(service);
            await _context.SaveChangesAsync();
        }

        public async Task EditProviderService(ProviderservicesDTO providerservicesDTO, string userId, string serviceId)
        {
            var service = await _context.providerServices
            .Include(g => g.ServicesImages)
            .FirstOrDefaultAsync(g => g.Id == serviceId && g.serviceProviderId == userId);

            if (service == null)
            {
                throw new ProviderServiceNotFoundException("Provider not found.");
            }
            _mapper.Map(providerservicesDTO, service);
            var path = @"Images/ServiceProvider/MyServices/";

            if (providerservicesDTO.Images != null && providerservicesDTO.Images.Any())
            {
                foreach (var image in service.ServicesImages)
                {
                    await _imageService.DeleteFileAsync(image.Img);
                }
                service.ServicesImages.Clear();

                if (providerservicesDTO.Images != null && providerservicesDTO.Images.Any())
                {

                    foreach (var image in providerservicesDTO.Images)
                    {
                        var imagePath = await _imageService.SaveFileAsync(image, path);
                        service.ServicesImages.Add(new ProviderServicesImage
                        {
                            Img = imagePath,
                            serviceId = service.Id
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProviderServices>> GetAllProviderService()
        {
            var service = await _context.providerServices
                .Include(i => i.ServicesImages)
                .Include(i => i.serviceProvider)
                .ToListAsync();

            if (service == null || !service.Any())
            {
                return new List<ProviderServices>();
            }
            foreach (var item in service)
            {
                item.ServicesImages = item.ServicesImages.Where(img => img.Img.StartsWith("Images/ServiceProvider/MyServices/")).ToList();

            }

            return service;
        }

        public async Task<ProviderServices> GetProviderServiceByIdAsync(string serviceId, string userId)
        {
            var provider = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            var service = await _context.providerServices
                .Include(g => g.ServicesImages)
                .Include(g => g.serviceProvider)
                .FirstOrDefaultAsync(g => g.Id ==serviceId && g.serviceProviderId == provider.Id);

            if (service == null)
            {
                throw new ProviderServiceNotFoundException("Service not found.");
            }
            service.ServicesImages = service.ServicesImages.Where(img => img.Img.StartsWith("Images/ServiceProvider/MyServices/")).ToList();
            return service;
        }
    }
}
