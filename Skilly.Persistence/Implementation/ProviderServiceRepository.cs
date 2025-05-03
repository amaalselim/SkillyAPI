using AutoMapper;
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
            service.ServiceRequestTime = DateOnly.FromDateTime(DateTime.Now);
            service.providerImg = user.Img;
            service.uId = user.UserId;

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
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId== userId);
            var service = await _context.providerServices
            .Include(g => g.ServicesImages)
            .FirstOrDefaultAsync(g => g.Id == serviceId && g.uId == user.Id);

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
            var services = await _context.providerServices
                .Include(i => i.ServicesImages)
                .Include(i => i.serviceProvider)
                .ToListAsync();

            if (services == null || !services.Any())
            {
                return new List<ProviderServices>();
            }

            var serviceDtos = services.Select(item => new ProviderServices
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ServiceRequestTime = item.ServiceRequestTime,
                Price = item.Price,
                Deliverytime = item.Deliverytime,
                categoryId=item.categoryId,
                Notes = item.Notes,
                serviceProviderId = item.serviceProviderId,
                ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                providerImg= item.serviceProvider.Img,
                Images = item.ServicesImages?
                    .Select(img => img.Img)
                    .ToList() ?? new List<string>()
            }).ToList();

            return serviceDtos;
        }


        public async Task<ProviderServices> GetProviderServiceByIdAsync(string serviceId)
        {
            //var provider = await _context.serviceProviders.ToListAsync();
            var service = await _context.providerServices
                .Include(c => c.serviceProvider)
                .Include(g => g.ServicesImages)
                .Include(g => g.offerSalaries)
                .FirstOrDefaultAsync(g => g.Id == serviceId);

            if (service == null)
            {
                throw new ProviderServiceNotFoundException("Service not found.");
            }

            var serviceDto = new ProviderServices
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                ServiceRequestTime = service.ServiceRequestTime,
                Price = service.Price,
                Deliverytime = service.Deliverytime,
                categoryId = service.categoryId,
                Notes = service.Notes,
                serviceProviderId = service.serviceProviderId,
                ServiceProviderName = service.serviceProvider.FirstName + " " + service.serviceProvider.LastName,
                providerImg = service.serviceProvider.Img,
                Images = service.ServicesImages?
                    .Select(img => img.Img)
                    .ToList() ?? new List<string>(),
                offerSalaries = service.offerSalaries?.ToList() ?? new List<OfferSalary>()
            };

            return serviceDto;
        }
        public async Task<List<ProviderServices>> GetAllservicesbyCategoryId(string categoryId)
        {
            var services = await _context.providerServices
                .Include(c => c.serviceProvider)
                .Include(c => c.ServicesImages)
                .Include(c => c.offerSalaries)
                .Where(c => c.categoryId == categoryId)
                .ToListAsync();

            if (services == null || !services.Any())
            {
                return new List<ProviderServices>();
            }

            var serviceDtos = services.Select(item => new ProviderServices
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ServiceRequestTime = item.ServiceRequestTime,
                Price = item.Price,
                Deliverytime = item.Deliverytime,
                Notes = item.Notes,
                categoryId = item.categoryId,
                serviceProviderId = item.serviceProviderId,
                ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                providerImg = item.serviceProvider.Img,
                Images = item.ServicesImages?
                    .Select(img => img.Img)
                    .ToList() ?? new List<string>(),
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>()
            }).ToList();

            return serviceDtos;
        }




        public async Task<IEnumerable<ProviderServices>> GetAllServicesByproviderId(string userId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return new List<ProviderServices>();
            }

            var services = await _context.providerServices
                .Include(c => c.serviceProvider)
                .Include(c => c.ServicesImages)
                .Include(c => c.offerSalaries)
                .Where(c => c.serviceProviderId == user.Id)
                .ToListAsync();

            if (services == null || !services.Any())
            {
                return new List<ProviderServices>();
            }

            var serviceDtos = services.Select(item => new ProviderServices
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ServiceRequestTime = item.ServiceRequestTime,
                Price = item.Price,
                Deliverytime = item.Deliverytime,
                Notes = item.Notes,
                categoryId = item.categoryId,
                serviceProviderId = item.serviceProviderId,
                ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                providerImg = item.serviceProvider.Img,
                Images = item.ServicesImages?
                    .Select(img => img.Img)
                    .ToList() ?? new List<string>(),
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>()
            }).ToList();

            return serviceDtos;
        }

    }
}
