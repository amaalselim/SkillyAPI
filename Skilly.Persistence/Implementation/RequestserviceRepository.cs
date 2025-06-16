using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs.User;
using Skilly.Application.Exceptions;
using Skilly.Core.Entities;
using Skilly.Core.Enums;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
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
        private readonly FirebaseV1Service _firebase;

        public RequestserviceRepository(
            ApplicationDbContext context,
            IImageService imageService,
            IMapper mapper,
            FirebaseV1Service firebaseV1Service,
            ILogger<RequestserviceRepository> logger,
            FirebaseV1Service firebase)
        {
            _context = context;
            _imageService = imageService;
            _mapper = mapper;
            _firebaseV1Service = firebaseV1Service;
            _logger = logger;
            _firebase = firebase;
        }

        public async Task AddRequestService(requestServiceDTO requestServiceDTO, string userId)
        {
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new UserProfileNotFoundException("User not found.");
            }
            var path = @"Images/UserProfile/RequestServices/";
            //var service = _mapper.Map<RequestService>(requestServiceDTO);
            var service = new RequestService
            {
                Name = requestServiceDTO.Name,
                Price = requestServiceDTO.Price,
                Deliverytime = requestServiceDTO.Deliverytime,
                startDate = requestServiceDTO.startDate,
                categoryId = requestServiceDTO.categoryId,
                Notes = requestServiceDTO.Notes,
                userId = user.Id,
                userImg = user.Img,
                uId = user.UserId

            };
            var sid = service.Id;
            service.ServiceRequestTime = DateOnly.FromDateTime(DateTime.Now);
            service.userImg = user.Img;
            service.uId = user.UserId;
            service.ServiceStatus = ServiceStatus.Posted;

            await _context.requestServices.AddAsync(service);
            await _context.SaveChangesAsync();

            List<string> imagePaths = new List<string>();
            if (requestServiceDTO.video != null)
            {
                if(requestServiceDTO.video.ContentType != "video/mp4")
                {
                    throw new Exception("Invalid video format. Only mp4 is allowed.");
                }

                var videoPath = await _imageService.SaveFileAsync(requestServiceDTO.video, path);
                service.video = videoPath;
            }

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
                        userImg = cat.Img,
                        serviceId=sid,
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
                .FirstOrDefaultAsync(g => g.Id == requestId && g.uId == user.UserId);

            if (service == null)
            {
                throw new Exception("Service not found.");
            }
            var path = @"Images/UserProfile/RequestServices";


            service.requestServiceImages.Clear();

            var payments = await _context.payments
            .Where(p => p.RequestServiceId == requestId)
                .ToListAsync();
            _context.payments.RemoveRange(payments);


            var bookings = await _context.offerSalaries
            .Where(b => b.requestserviceId == requestId)
                .ToListAsync();
            _context.offerSalaries.RemoveRange(bookings);

            var not = await _context.notifications
            .Where(b => b.serviceId == requestId)
                .ToListAsync();
            _context.notifications.RemoveRange(not);
            _context.requestServices.Remove(service);
            await _context.SaveChangesAsync();
        }

        public async Task EditRequestService(EditRequestServiceDTO requestServiceDTO, string userId, string requestId)
        {
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new UserProfileNotFoundException("User not found.");
            }

            var service = await _context.requestServices
                .Include(c => c.UserProfile)
                .Include(g => g.requestServiceImages)
                .FirstOrDefaultAsync(g => g.Id == requestId && g.uId == user.UserId);

            if (service == null)
            {
                throw new Exception("Service not found.");
            }


            if (requestServiceDTO.ImagesToDeleteIds != null && requestServiceDTO.ImagesToDeleteIds.Any())
            {
                var imagesToDelete = service.requestServiceImages
                    .Where(img => requestServiceDTO.ImagesToDeleteIds.Contains(img.Id))
                    .ToList();

                foreach (var img in imagesToDelete)
                {
                    
                    await _imageService.DeleteFileAsync(img.Img);

                   
                    service.requestServiceImages.Remove(img);
                }
                await _context.SaveChangesAsync();
            }

            service.Name = requestServiceDTO.Name;
            service.Price = requestServiceDTO.Price;
            service.Deliverytime = requestServiceDTO.Deliverytime;
            service.startDate = requestServiceDTO.startDate;
            service.categoryId = requestServiceDTO.categoryId;
            service.Notes = requestServiceDTO.Notes;

            var sid = service.Id;
            service.userId = user.Id;
            service.ServiceRequestTime = DateOnly.FromDateTime(DateTime.Now);
            service.userImg = user.Img;
            service.uId = user.UserId;
            service.ServiceStatus = ServiceStatus.Posted;

            var path = @"Images/UserProfile/RequestServices/";

            if (requestServiceDTO.video != null)
            {
                if (requestServiceDTO.video.ContentType != "video/mp4")
                {
                    throw new Exception("Invalid video format. Only mp4 is allowed.");
                }

                if (!string.IsNullOrEmpty(service.video))
                {
                    await _imageService.DeleteFileAsync(service.video);
                }
                await _context.SaveChangesAsync();

                var videoPath = await _imageService.SaveFileAsync(requestServiceDTO.video, path);
                service.video = videoPath;
            }

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
        }



        public async Task<IEnumerable<RequestService>> GetAllRequests()
        {
            var services = await _context.requestServices
                .Include(c => c.UserProfile)
                .ThenInclude(sp => sp.User)
                .Include(c => c.requestServiceImages)
                .Include(c => c.offerSalaries)
                .ToListAsync();

            if (services == null || !services.Any())
            {
                return new List<RequestService>();
            }

            var serviceDtos = services.Select(item =>
            {

                return new RequestService
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price =item.Price,
                    Deliverytime =item.Deliverytime,
                    startDate = item.startDate,
                    categoryId = item.categoryId,
                    Notes = item.Notes,
                    ServiceRequestTime = item.ServiceRequestTime,
                    userId = item.userId,
                    userImg = item.UserProfile.Img,
                    userName = item.UserProfile.FirstName + " " + item.UserProfile.LastName,
                    Images = item.requestServiceImages?.Select(img => new requestServiceImage
                    {
                        Id = img.Id,
                        Img = img.Img
                    }).ToList() ?? new List<requestServiceImage>(),
                    video = item.video,
                    offerSalaries = item.offerSalaries.Where(p => p.Status == 0)?.ToList() ?? new List<OfferSalary>(),
                    OffersCount = item.offerSalaries?.Count(p => p.Status == 0) ?? 0
                   
                };
            }).ToList();
            return serviceDtos;
        }

        public async Task<IEnumerable<RequestService>> GetAllRequestsByUserId(string userId)
        {
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == userId || u.Id==userId);

            var services = await _context.requestServices
                .Include(c => c.UserProfile)
                .Include(c => c.requestServiceImages)
                .Include(c => c.offerSalaries)
                .Where(g => g.userId == user.Id)
                .ToListAsync();

            if (services == null || !services.Any())
            {
                return new List<RequestService>();
            }

            var serviceDtos = services.Select(item =>
            {
                var acceptedOffer = item.offerSalaries
                    .FirstOrDefault(o => o.Status == OfferStatus.Accepted && o.userId==userId);

                return new RequestService
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = acceptedOffer != null ? acceptedOffer.Salary : item.Price,
                    Deliverytime = acceptedOffer != null ? acceptedOffer.Deliverytime : item.Deliverytime,
                    startDate = item.startDate,
                    categoryId = item.categoryId,
                    Notes = item.Notes,
                    ServiceRequestTime = item.ServiceRequestTime,
                    userId = item.userId,
                    userImg = item.UserProfile.Img,
                    userName = item.UserProfile.FirstName + " " + item.UserProfile.LastName,
                    Images = item.requestServiceImages?.Select(img => new requestServiceImage
                    {
                        Id = img.Id,
                        Img = img.Img
                    }).ToList() ?? new List<requestServiceImage>(),
                    video = item.video,
                    offerSalaries = item.offerSalaries.Where(p => p.Status == 0)?.ToList() ?? new List<OfferSalary>(),
                    OffersCount = item.offerSalaries?.Count(p => p.Status == 0) ?? 0,
                };
            }).ToList();

            return serviceDtos;
        }
        public async Task<IEnumerable<RequestService>> GetAllRequestsByCategoryId(string userId, string sortBy, double? userLat = null, double? userLon = null)
        {
            var provider = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);

            var services = await _context.requestServices
                .Include(c => c.UserProfile)
                    .ThenInclude(u => u.User)
                .Include(c => c.requestServiceImages)
                .Include(c => c.offerSalaries)
                .Where(g => g.categoryId == provider.categoryId && g.ServiceStatus == ServiceStatus.Posted)
                .ToListAsync();

            if (services == null || !services.Any())
            {
                return new List<RequestService>();
            }

            var serviceDtos = services.Select(item =>
            {
                var acceptedOffer = item.offerSalaries
                    .FirstOrDefault(o => o.Status == OfferStatus.Accepted && o.userId == userId);

                return new RequestService
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = acceptedOffer != null ? acceptedOffer.Salary : item.Price,
                    Deliverytime = acceptedOffer != null ? acceptedOffer.Deliverytime : item.Deliverytime,
                    startDate = item.startDate,
                    categoryId = item.categoryId,
                    Notes = item.Notes,
                    ServiceRequestTime = item.ServiceRequestTime,
                    userId = item.userId,
                    userImg = item.UserProfile.Img,
                    userName = item.UserProfile.FirstName + " " + item.UserProfile.LastName,
                    Images = item.requestServiceImages?.Select(img => new requestServiceImage
                    {
                        Id = img.Id,
                        Img = img.Img
                    }).ToList() ?? new List<requestServiceImage>(),
                    video = item.video,
                    offerSalaries = item.offerSalaries.Where(p => p.Status == 0)?.ToList() ?? new List<OfferSalary>(),
                    OffersCount = item.offerSalaries?.Count(p => p.Status == 0) ?? 0,
                    Distance = (userLat != null && userLon != null)
                        ? GeoHelper.GetDistance(userLat.Value, userLon.Value, item?.UserProfile?.User?.Latitude, item?.UserProfile?.User?.Longitude).GetValueOrDefault()
                        : 0
                };
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


        public async Task<RequestService> GetRequestById(string requestId,string currentUserId)
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
            var acceptedOffer = service.offerSalaries
                    .FirstOrDefault(o => o.Status == OfferStatus.Accepted && o.userId == currentUserId);

            var serviceDto = new RequestService
            {
                Id = service.Id,
                ServiceRequestTime = service.ServiceRequestTime,
                Name = service.Name,
                Price = acceptedOffer!=null? acceptedOffer.Salary : service.Price,
                Deliverytime = acceptedOffer != null ? acceptedOffer.Deliverytime : service.Deliverytime,
                startDate = service.startDate,
                categoryId = service.categoryId,
                Notes = service.Notes,
                userId = service.userId,
                userName = service.UserProfile.FirstName + " " + service.UserProfile.LastName,
                userImg = service.UserProfile.Img,
                Images = service.requestServiceImages?.Select(img => new requestServiceImage
                {
                    Id = img.Id,
                    Img = img.Img
                }).ToList() ?? new List<requestServiceImage>(),
                video = service.video,
                offerSalaries = service.offerSalaries.Where(p => p.Status == 0)?.ToList() ?? new List<OfferSalary>(),
                OffersCount =service.offerSalaries?.Count(p => p.Status == 0) ?? 0,
            };

            return serviceDto;
        }
        public async Task<IEnumerable<RequestService>> GetSortedUserAsync(
     string sortBy, string currentUserId, double? userLat = null, double? userLon = null)
        {
            var services = await _context.requestServices
                .Include(c => c.UserProfile)
                    .ThenInclude(u => u.User)
                .Include(c => c.requestServiceImages)
                .Include(c => c.offerSalaries)
                .ToListAsync();

            if (services == null || !services.Any())
            {
                return new List<RequestService>();
            }

            var serviceDtos = services.Select(item =>
            {
                var acceptedOffer = item.offerSalaries
                    .FirstOrDefault(o => o.Status == OfferStatus.Accepted && o.userId == currentUserId);

                return new RequestService
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = acceptedOffer != null ? acceptedOffer.Salary : item.Price,
                    Deliverytime = acceptedOffer != null ? acceptedOffer.Deliverytime : item.Deliverytime,
                    startDate = item.startDate,
                    categoryId = item.categoryId,
                    Notes = item.Notes,
                    ServiceRequestTime = item.ServiceRequestTime,
                    userId = item.userId,
                    userImg = item.UserProfile.Img,
                    userName = item.UserProfile.FirstName + " " + item.UserProfile.LastName,
                    Images = item.requestServiceImages?.Select(img => new requestServiceImage
                    {
                        Id = img.Id,
                        Img = img.Img
                    }).ToList() ?? new List<requestServiceImage>(),
                    video = item.video,
                    offerSalaries = item.offerSalaries.Where(p => p.Status == 0)?.ToList() ?? new List<OfferSalary>(),
                    OffersCount = item.offerSalaries?.Count(p => p.Status == 0) ?? 0,
                    Distance = (userLat != null && userLon != null)
                        ? GeoHelper.GetDistance(userLat.Value, userLon.Value, item?.UserProfile?.User?.Latitude, item?.UserProfile?.User?.Longitude).GetValueOrDefault()
                        : 0
                };
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

        public async Task AcceptService(string requestId, string userId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            var service = await _context.requestServices
                .Include(c => c.UserProfile)
                .Include(c => c.requestServiceImages)
                .FirstOrDefaultAsync(g => g.Id == requestId);
            if (service == null)
            {
                throw new Exception("Service not found.");
            }
            service.providerId = user.UserId; 
            string title = "قبول خدمة";
            string body = $"تم قبول خدمتك {service.Name} من قبل موفر الخدمة {user.FirstName} {user.LastName}، برجاء الذهاب للدفع.";
            
            var userprofile=await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId== service.uId);
            var userr=await _context.users.FirstOrDefaultAsync(p=>p.Id==userprofile.UserId);
            if (service != null)
            {
                await _firebase.SendNotificationAsync(
                    userr.FcmToken,
                    title,
                    body
                );

                _context.notifications.Add(new Notifications
                {
                    UserId = userr.Id,
                    Title = title,
                    Body = body,
                    userImg = userprofile.Img,
                    serviceId = service.Id,
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                });
            }
            await _context.SaveChangesAsync();
        }
        public async Task<object?> TrackRequestServiceAsync(string serviceId,string userId)
        {
            var requestService = await _context.requestServices
                .Include(r => r.UserProfile)
                .FirstOrDefaultAsync(r => r.Id == serviceId &&r.uId==userId);

            if (requestService == null)
                return null;

            return new
            {
                ServiceId = requestService.Id,
                ServiceName = requestService.Name,
                Status = requestService.ServiceStatus,
                StatusArabic = GetArabicStatus(requestService.ServiceStatus),
                UserId = requestService.userId,
                UserName = requestService.UserProfile.FirstName + " " + requestService.UserProfile.LastName,
                UpdatedAt = DateTime.Now
            };
        }

        private string GetArabicStatus(ServiceStatus status)
        {
            return status switch
            {
                ServiceStatus.Posted => "تم النشر",
                ServiceStatus.Paid => "تم الدفع",
                ServiceStatus.InProgress => "قيد التنفيذ",
                ServiceStatus.Completed => "تم التنفيذ",
                ServiceStatus.Delivered => "تم الاستلام",
                _ => "غير معروف"
            };
        }
    }
}
