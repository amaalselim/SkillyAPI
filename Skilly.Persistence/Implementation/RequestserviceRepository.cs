using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
using Skilly.Core.Entities;
using Skilly.Core.Enums;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            service.ServiceRequestTime = DateOnly.FromDateTime(DateTime.Now);
            service.userImg = user.Img;
            service.uId = user.UserId;
            service.ServiceStatus = ServiceStatus.Posted;

            await _context.requestServices.AddAsync(service);
            await _context.SaveChangesAsync();

            List<string> imagePaths = new List<string>();

            if (requestServiceDTO.Images != null && requestServiceDTO.Images.Any())
            {
                foreach (var image in requestServiceDTO.Images)
                {
                    var imagePath = await _imageService.SaveFileAsync(image, path);
                    imagePaths.Add(imagePath);
                }

                service.requestServiceImages = imagePaths.Select(imgPath => new requestServiceImage
                {
                    Img = imgPath,
                    requestServiceId = service.Id
                }).ToList();
            }
            user.Points += 20;
            await _context.SaveChangesAsync();


            var providers = await _context.serviceProviders
                .Where(u => u.categoryId == service.categoryId && u.User.FcmToken != null)
                .Include(p => p.User)
                .ToListAsync();
            var cat = await _context.categories.FirstOrDefaultAsync(c => c.Id == service.categoryId);

            string title = "طلب خدمة جديد";
            string body = $"تم نشر طلب جديد في قسم {cat?.Name ?? "القسم الخاص بك"}، يمكنك مشاهدته الآن.";

            foreach (var provider in providers)
            {
                try
                {
                    await _firebaseV1Service.SendNotificationAsync(
                        provider.User.FcmToken,
                        title,
                        body
                    );

                    _context.notifications.Add(new Notifications
                    {
                        UserId = provider.UserId,
                        Title = title,
                        Body = body,
                        userImg = provider.Img,
                        CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send notification to provider {provider.Id}: {ex.Message}");
                }
            }
            await _context.SaveChangesAsync();
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
           .FirstOrDefaultAsync(g => g.Id == requestId && g.uId == user.Id);

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

        public async Task<IEnumerable<RequestService>> GetAllRequests(double? userLat = null, double? userLng = null)
        {
            var services = await _context.requestServices
                .Include(c => c.UserProfile)
                .ThenInclude(sp => sp.User)
                .Include(c => c.requestServiceImages)
                .ToListAsync();

            if (services == null || !services.Any())
            {
                return new List<RequestService>();
            }

            var serviceDtos = services.Select(item => new RequestService
            {
                Id = item.Id,
                Name = item.Name,

                Price = item.Price,
                Deliverytime = item.Deliverytime,
                startDate = item.startDate,
                categoryId = item.categoryId,
                Notes = item.Notes,
                ServiceRequestTime = item.ServiceRequestTime,
                userId = item.userId,
                userImg = item.UserProfile.Img,
                userName = item.UserProfile.FirstName + " " + item.UserProfile.LastName,
                Images = item.requestServiceImages
                    .Select(img => img.Img)
                    .ToList(),
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                OffersCount = item.offerSalaries?.Count ?? 0,
                Distance = (userLat != null && userLng != null)
                    ? GeoHelper.GetDistance(userLat.Value, userLng.Value, item?.UserProfile.User?.Latitude, item?.UserProfile.User?.Longitude).GetValueOrDefault()
                    : 0
            }).ToList();

            if (userLat != null && userLng != null)
            {
                serviceDtos = serviceDtos
                    .OrderBy(s => s.Distance)
                    .ToList();
            }

            return serviceDtos;
        }
        public async Task<IEnumerable<RequestService>> GetAllRequestsByUserId(string userId)
        {
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            var services = await _context.requestServices
                .Include(c => c.UserProfile)
                .Include(c => c.requestServiceImages)
                .Where(g => g.userId == user.Id)
                .ToListAsync();

            if (services == null || !services.Any())
            {
                return new List<RequestService>();
            }

            var serviceDtos = services.Select(item => new RequestService
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Deliverytime = item.Deliverytime,
                startDate = item.startDate,
                categoryId = item.categoryId,
                Notes = item.Notes,
                ServiceRequestTime = item.ServiceRequestTime,
                userId = item.userId,
                userImg = item.UserProfile.Img,
                userName = item.UserProfile.FirstName + " " + item.UserProfile.LastName,
                Images = item.requestServiceImages
                    .Select(img => img.Img)
                    .ToList(),
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                OffersCount = item.offerSalaries?.Count ?? 0
            }).ToList();

            return serviceDtos;
        }

        public async Task<IEnumerable<RequestService>> GetAllRequestsByCategoryId(string userId)
        {
            var provider = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            var services = await _context.requestServices
                .Include(c => c.UserProfile)
                .Include(c => c.requestServiceImages)
                .Where(g => g.categoryId == provider.categoryId && g.ServiceStatus == ServiceStatus.Posted)
                .ToListAsync();

            if (services == null || !services.Any())
            {
                return new List<RequestService>();
            }

            var serviceDtos = services.Select(item => new RequestService
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Deliverytime = item.Deliverytime,
                startDate = item.startDate,
                categoryId = item.categoryId,
                Notes = item.Notes,
                ServiceRequestTime = item.ServiceRequestTime,
                userId = item.userId,
                userImg = item.UserProfile.Img,
                userName = item.UserProfile.FirstName + " " + item.UserProfile.LastName,
                Images = item.requestServiceImages
                    .Select(img => img.Img)
                    .ToList(),
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                OffersCount = item.offerSalaries?.Count ?? 0
            }).ToList();

            return serviceDtos;
        }


        public async Task<RequestService> GetRequestById(string requestId)
        {
            var service = await _context.requestServices
                .Include(g => g.requestServiceImages)
                .Include(g => g.UserProfile)
                .Include(g => g.offerSalaries)
                .FirstOrDefaultAsync(g => g.Id == requestId);

            if (service == null)
            {
                throw new Exception("Service not found.");
            }

            var serviceDto = new RequestService
            {
                Id = service.Id,
                ServiceRequestTime = service.ServiceRequestTime,
                Name = service.Name,
                Price = service.Price,
                Deliverytime = service.Deliverytime,
                startDate = service.startDate,
                categoryId = service.categoryId,
                Notes = service.Notes,
                userId = service.userId,
                userName = service.UserProfile.FirstName + " " + service.UserProfile.LastName,
                userImg = service.UserProfile.Img,
                Images = service.requestServiceImages?
                    .Select(img => img.Img)
                    .ToList() ?? new List<string>(),
                offerSalaries = service.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                OffersCount = service.offerSalaries?.Count ?? 0
            };

            return serviceDto;
        }
        public async Task<IEnumerable<RequestService>> GetSortedUserAsync(
                string sortBy, double? userLat = null, double? userLon = null)
        {
            var services = await _context.requestServices
                .Include(c => c.UserProfile)
                .Include(c => c.requestServiceImages)
                .ToListAsync();

            if (services == null || !services.Any())
            {
                return new List<RequestService>();
            }

            var serviceDtos = services.Select(item => new RequestService
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Deliverytime = item.Deliverytime,
                startDate = item.startDate,
                categoryId = item.categoryId,
                Notes = item.Notes,
                ServiceRequestTime = item.ServiceRequestTime,
                userId = item.userId,
                userImg = item.UserProfile.Img,
                userName = item.UserProfile.FirstName + " " + item.UserProfile.LastName,
                Images = item.requestServiceImages
                    .Select(img => img.Img)
                    .ToList(),
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                OffersCount = item.offerSalaries?.Count ?? 0,
                Distance = (userLat != null && userLon != null)
                    ? GeoHelper.GetDistance(userLat.Value, userLon.Value, item?.UserProfile.User?.Latitude, item.UserProfile.User?.Longitude).GetValueOrDefault()
                    : 0

            });

            serviceDtos = string.IsNullOrEmpty(sortBy) || sortBy.ToLower() == "nearest"
                ? (userLat != null && userLon != null ? serviceDtos.OrderBy(s => s.Distance) : serviceDtos)
                : sortBy.ToLower() switch
                {
                    "price-asc" => serviceDtos.OrderBy(s => s.Price),
                    "latest" => serviceDtos.OrderByDescending(s => s.ServiceRequestTime),
                    _ => serviceDtos.OrderBy(s => s.Price)
                };

            return serviceDtos.ToList();
        }

    }
}
