using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    public class RequestserviceRepository : IRequestserviceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;
        private readonly FirebaseV1Service _firebaseV1Service;
        private readonly ILogger<RequestserviceRepository> _logger;

        public RequestserviceRepository(
            ApplicationDbContext context,
            IImageService imageService,
            IMapper mapper,
            FirebaseV1Service firebaseV1Service,
            ILogger<RequestserviceRepository> logger)
        {
            _context = context;
            _imageService = imageService;
            _mapper = mapper;
            _firebaseV1Service = firebaseV1Service;
            _logger = logger;
        }
        public async Task AddRequestService(requestServiceDTO requestServiceDTO, string userId)
        {
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new UserProfileNotFoundException("User not found.");
            }
            var path = @"Images/UserProfile/RequestServices/";
            var service = _mapper.Map<RequestService>(requestServiceDTO);
            service.userId = user.Id;
            service.ServiceRequestTime = DateTime.UtcNow;

            await _context.requestServices.AddAsync(service);
            await _context.SaveChangesAsync();

            if (requestServiceDTO.Images != null && requestServiceDTO.Images.Any())
            {
                foreach (var image in requestServiceDTO.Images)
                {
                    var imagePath = await _imageService.SaveFileAsync(image, path);

                    service.requestServiceImages.Add(new requestServiceImage
                    {
                        Img = imagePath,
                        requestServiceId = service.Id
                    });
                }
            }

            
            await _context.SaveChangesAsync();

            var providers = await _context.serviceProviders.Where(u => u.categoryId == service.categoryId && u.User.FcmToken != null)
                .Include(p=>p.User)
                .ToListAsync();

            foreach (var provider in providers)
            {
                try
                {
                    await _firebaseV1Service.SendNotificationAsync(
                        provider.User.FcmToken,
                        "New Service Request",
                        $"There's a new service in your category. Check it out!"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send notification to provider {provider.Id}: {ex.Message}");
                }
            }


        }

        public async Task DeleteRequestServiceAsync(string requestId, string userId)
        {
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            var service = await _context.requestServices
                .Include(c => c.UserProfile)
            .Include(g => g.requestServiceImages)
                .FirstOrDefaultAsync(g => g.Id == requestId && g.userId == user.Id);

            if (service == null)
            {
                throw new Exception("Service not found.");
            }
            var path = @"Images/UserProfile/RequestServices";


            service.requestServiceImages.Clear();
            _context.requestServices.Remove(service);
            await _context.SaveChangesAsync();
        }

        public async Task EditRequestService(requestServiceDTO requestServiceDTO, string userId, string requestId)
        {
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            var service = await _context.requestServices
                .Include(c => c.UserProfile)
           .Include(g => g.requestServiceImages)
           .FirstOrDefaultAsync(g => g.Id == requestId && g.userId == user.Id);

            if (service == null)
            {
                throw new UserProfileNotFoundException("User not found.");
            }
            _mapper.Map(requestServiceDTO, service);
            var path = @"Images/UserProfile/RequestServices";

            if (requestServiceDTO.Images != null && requestServiceDTO.Images.Any())
            {
                foreach (var image in service.requestServiceImages)
                {
                    await _imageService.DeleteFileAsync(image.Img);
                }
                service.requestServiceImages.Clear();

                if (requestServiceDTO.Images != null && requestServiceDTO.Images.Any())
                {

                    foreach (var image in requestServiceDTO.Images)
                    {
                        var imagePath = await _imageService.SaveFileAsync(image, path);
                        service.requestServiceImages.Add(new requestServiceImage
                        {
                            Img = imagePath,
                            requestServiceId = service.Id
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RequestService>> GetAllRequests()
        {
            var service = await _context.requestServices
                .Include(c=>c.UserProfile)
                //.Include(c => c.offerSalaries)
                .Include(c=>c.requestServiceImages)
               .ToListAsync();

            if (service == null || !service.Any())
            {
                return new List<RequestService>();
            }
            foreach (var item in service)
            {
                item.userName = item.UserProfile.FirstName + " " + item.UserProfile.LastName;
                item.requestServiceImages = item.requestServiceImages.Where(img => img.Img.StartsWith("Images/UserProfile/RequestServices/")).ToList();
                //item.offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>();
            }

            return service;
        }

        public async Task<IEnumerable<RequestService>> GetAllRequestsByUserId(string userId)
        {
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            var service = await _context.requestServices
                .Include(c => c.requestServiceImages)
                .Include(c => c.offerSalaries)
                .Where(c=>c.userId==user.Id)
               .ToListAsync();
            
            if (service == null || !service.Any())
            {
                return new List<RequestService>();
            }
            foreach (var item in service)
            {
                item.userName = item.UserProfile.FirstName + " " + item.UserProfile.LastName;
                item.requestServiceImages = item.requestServiceImages.Where(img => img.Img.StartsWith("Images/UserProfile/RequestServices/")).ToList();
                item.offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>();
            }

            return service;
        }

        public async Task<RequestService> GetRequestById(string requestId, string userId)
        {
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            var service = await _context.requestServices
                .Include(g => g.requestServiceImages)
                .Include(g => g.UserProfile)
                .Include(g=>g.offerSalaries)
                .FirstOrDefaultAsync(g => g.Id == requestId&& g.userId == user.Id);

            if (service == null)
            {
                throw new Exception("Service not found.");
            }
            service.userName=service.UserProfile.FirstName+" "+service.UserProfile.LastName;
            service.requestServiceImages = service.requestServiceImages.Where(img => img.Img.StartsWith("Images/UserProfile/RequestServices/")).ToList();
            service.offerSalaries = service.offerSalaries?.ToList() ?? new List<OfferSalary>();
            return service;
        }
    }
}
