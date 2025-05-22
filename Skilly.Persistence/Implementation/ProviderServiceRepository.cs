using AutoMapper;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
using Skilly.Core.Entities;
using Skilly.Core.Enums;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using Skilly.Persistence.Migrations;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
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
        private readonly FirebaseV1Service _firebase;

        public ProviderServiceRepository(ApplicationDbContext context, IMapper mapper, IImageService imageService, FirebaseV1Service firebase)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
            _firebase = firebase;
        }
        public async Task<IEnumerable<ProviderServices>> GetAllProviderServiceDiscounted(double? userLat = null, double? userLng = null)
        {
            var services = await _context.providerServices
                .Include(i => i.ServicesImages)
                .Include(i => i.serviceProvider)
                .ThenInclude(sp => sp.User)
                .Where(p => p.PriceDiscount != null && p.PriceDiscount > 0)
                .ToListAsync();

            if (services == null || !services.Any())
                return new List<ProviderServices>();

            var serviceDtos = services.Select(item => new ProviderServices
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ServiceRequestTime = item.ServiceRequestTime,
                Price = item.Price,
                PriceDiscount = item.PriceDiscount,
                Deliverytime = item.Deliverytime,
                categoryId = item.categoryId,
                Notes = item.Notes,
                serviceProviderId = item.serviceProviderId,
                ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                providerImg = item.serviceProvider.Img,
                video = item.video,
                Images = item.ServicesImages?.Select(img => img.Img).ToList() ?? new List<string>()
            }).ToList();

            return serviceDtos;

        }
        public async Task UseServiceDiscount(string serviceId, string userId)
        {
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            var service = await _context.providerServices.FirstOrDefaultAsync(u => u.Id == serviceId);
            if (user.Points >= 100)
            {
                decimal finalPrice = service.Price * 0.85m;
                service.PriceDiscount = finalPrice;
                user.Points -= 100;
                user.useDiscount = true;
            }
            else
            {
                throw new ServiceProviderNotFoundException("You don't have enough points to use the discount.");
            }
            await _context.SaveChangesAsync();
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
            service.categoryId = user.categoryId;
            service.ServiceStatus = ServiceStatus.Posted;
            if(providerservicesDTO.video != null)
            {
                if (providerservicesDTO.video != null)
                {
                    if (providerservicesDTO.video.ContentType != "video/mp4")
                    {
                        throw new InvalidOperationException("Invalid video format. Only mp4 is allowed.");
                    }
                    var videoPath = await _imageService.SaveFileAsync(providerservicesDTO.video, path);
                    service.video = videoPath;
                }
            }

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
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            var service = await _context.providerServices
            .Include(g => g.ServicesImages)
            .FirstOrDefaultAsync(g => g.Id == serviceId && g.uId == user.Id);

            if (service == null)
            {
                throw new ProviderServiceNotFoundException("Provider not found.");
            }
            _mapper.Map(providerservicesDTO, service);
            service.serviceProviderId = user.Id;
            service.ServiceRequestTime = DateOnly.FromDateTime(DateTime.Now);
            service.providerImg = user.Img;
            service.uId = user.UserId;
            service.categoryId = user.categoryId;
            var path = @"Images/ServiceProvider/MyServices/";
            if (providerservicesDTO.video != null)
            {
                if (providerservicesDTO.video.ContentType != "video/mp4")
                {
                    throw new InvalidOperationException("Invalid video format. Only mp4 is allowed.");
                }
                var videoPath = await _imageService.SaveFileAsync(providerservicesDTO.video, path);
                service.video = videoPath;
            }
            
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
        public async Task<IEnumerable<ProviderServices>> GetAllProviderService(double? userLat = null, double? userLng = null)
        {
            var services = await _context.providerServices
                .Include(i => i.ServicesImages)
                .Include(i => i.serviceProvider)
                    .ThenInclude(sp => sp.User)
                .Include(i => i.offerSalaries)
                .Include(i => i.Reviews)
                .ToListAsync();

            if (services == null || !services.Any())
                return new List<ProviderServices>();

            var allUserIds = services.SelectMany(s => s.Reviews.Select(r => r.UserId)).Distinct().ToList();
            var allUsers = await _context.userProfiles
                .Where(u => allUserIds.Contains(u.UserId))
                .ToListAsync();

            var serviceDtos = services.Select(item => new ProviderServices
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ServiceRequestTime = item.ServiceRequestTime,
                Price = item.Price,
                PriceDiscount = item.PriceDiscount,
                Deliverytime = item.Deliverytime,
                categoryId = item.categoryId,
                Notes = item.Notes,
                serviceProviderId = item.serviceProviderId,
                ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                providerImg = item.serviceProvider.Img,
                Images = item.ServicesImages?.Select(img => img.Img).ToList() ?? new List<string>(),
                video = item.video,
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                CountOfOffers = item.offerSalaries?.Count ?? 0,
                Distance = (userLat != null && userLng != null)
                    ? GeoHelper.GetDistance(userLat.Value, userLng.Value, item?.serviceProvider.User?.Latitude, item?.serviceProvider.User?.Longitude).GetValueOrDefault()
                    : 0,
                Reviews = item.Reviews.Select(r =>
                {
                    var user = allUsers.FirstOrDefault(u => u.UserId == r.UserId);
                    return new Review
                    {
                        Id = r.Id,
                        serviceId = r.serviceId,
                        Feedback = r.Feedback,
                        Rating = r.Rating,
                        UserId = r.UserId,
                        UserName = user != null ? user.FirstName + " " + user.LastName : null,
                        UserImg = user != null ? user.Img : null
                    };
                }).ToList()
            }).ToList();

            if (userLat != null && userLng != null)
            {
                serviceDtos = serviceDtos.OrderBy(s => s.Distance).ToList();
            }

            return serviceDtos;
        }




        public async Task<ProviderServices> GetProviderServiceByIdAsync(string serviceId)
        {
            var service = await _context.providerServices
                .Include(c => c.serviceProvider)
                .Include(g => g.ServicesImages)
                .Include(g => g.offerSalaries)
                .FirstOrDefaultAsync(g => g.Id == serviceId);

            if (service == null)
                throw new ProviderServiceNotFoundException("Service not found.");

            var serviceDto = new ProviderServices
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                ServiceRequestTime = service.ServiceRequestTime,
                Price = service.Price,
                PriceDiscount = service.PriceDiscount,
                Deliverytime = service.Deliverytime,
                categoryId = service.categoryId,
                Notes = service.Notes,
                serviceProviderId = service.serviceProviderId,
                ServiceProviderName = service.serviceProvider.FirstName + " " + service.serviceProvider.LastName,
                providerImg = service.serviceProvider.Img,
                Images = service.ServicesImages?.Select(img => img.Img).ToList() ?? new List<string>(),
                video = service.video,
                offerSalaries = service.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                CountOfOffers = service.offerSalaries?.Count ?? 0
            };

            return serviceDto;
        }

        public async Task<List<ProviderServices>> GetAllservicesbyCategoryId(string categoryId, string sortBy, double? userLat = null, double? userLon = null)
        {
            var services = await _context.providerServices
                .Include(i => i.ServicesImages)
                .Include(i => i.serviceProvider)
                    .ThenInclude(sp => sp.User)
                .Include(i => i.offerSalaries)
                .Include(i => i.Reviews)
                .Where(c => c.categoryId == categoryId)
                .ToListAsync();

            if (services == null || !services.Any())
                return new List<ProviderServices>();

            var serviceDtos = services.Select(item => new ProviderServices
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ServiceRequestTime = item.ServiceRequestTime,
                Price = item.Price,
                PriceDiscount = item.PriceDiscount,
                Deliverytime = item.Deliverytime,
                Notes = item.Notes,
                categoryId = item.categoryId,
                serviceProviderId = item.serviceProviderId,
                ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                providerImg = item.serviceProvider.Img,
                Images = item.ServicesImages?.Select(img => img.Img).ToList() ?? new List<string>(),
                video = item.video,
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                CountOfOffers = item.offerSalaries?.Count ?? 0,
                Distance = (userLat != null && userLon != null)
                    ? GeoHelper.GetDistance(userLat.Value, userLon.Value, item?.serviceProvider.User?.Latitude, item?.serviceProvider.User?.Longitude).GetValueOrDefault()
                    : 0,
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

        public async Task<IEnumerable<ProviderServices>> GetAllServicesByproviderId(string userId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return new List<ProviderServices>();

            var services = await _context.providerServices
                .Include(c => c.serviceProvider)
                .Include(c => c.ServicesImages)
                .Include(c => c.offerSalaries)
                .Where(c => c.serviceProviderId == user.Id)
                .ToListAsync();

            if (services == null || !services.Any())
                return new List<ProviderServices>();

            var serviceDtos = services.Select(item => new ProviderServices
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ServiceRequestTime = item.ServiceRequestTime,
                Price = item.Price,
                PriceDiscount = item.PriceDiscount,
                Deliverytime = item.Deliverytime,
                Notes = item.Notes,
                categoryId = item.categoryId,
                serviceProviderId = item.serviceProviderId,
                ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                providerImg = item.serviceProvider.Img,
                Images = item.ServicesImages?.Select(img => img.Img).ToList() ?? new List<string>(),
                video = item.video,
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                CountOfOffers = item.offerSalaries?.Count ?? 0
            }).ToList();

            return serviceDtos;
        }

        public async Task<IEnumerable<ProviderServices>> GetSortedProviderServicesAsync(
                string sortBy, double? userLat = null, double? userLon = null)
        {
            var servicesQuery = _context.providerServices
                .Include(s => s.serviceProvider)
                .ThenInclude(sp => sp.User)
                .Include(s => s.ServicesImages)
                .Include(s => s.offerSalaries)
                .Include(s => s.Reviews)

                .AsQueryable();
            var allUserIds = servicesQuery.SelectMany(s => s.Reviews.Select(r => r.UserId)).Distinct().ToList();
            var allUsers = await _context.userProfiles
                .Where(u => allUserIds.Contains(u.UserId))
                .ToListAsync();

            var services = await servicesQuery.ToListAsync();

            var serviceDtos = services.Select(item => new ProviderServices
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ServiceRequestTime = item.ServiceRequestTime,
                Price = item.Price,
                PriceDiscount = item.PriceDiscount,
                Deliverytime = item.Deliverytime,
                Notes = item.Notes,
                categoryId = item.categoryId,
                serviceProviderId = item.serviceProviderId,
                ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                providerImg = item.serviceProvider.Img,
                Images = item.ServicesImages?.Select(img => img.Img).ToList() ?? new List<string>(),
                video = item.video,
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                CountOfOffers = item.offerSalaries?.Count ?? 0,
                Distance = (userLat != null && userLon != null)
                    ? GeoHelper.GetDistance(userLat.Value, userLon.Value, item?.serviceProvider.User?.Latitude, item?.serviceProvider.User?.Longitude).GetValueOrDefault()
                    : 0,
                Reviews = item.Reviews.Select(r =>
                {
                    var user = allUsers.FirstOrDefault(u => u.UserId == r.UserId);
                    return new Review
                    {
                        Id = r.Id,
                        serviceId = r.serviceId,
                        Feedback = r.Feedback,
                        Rating = r.Rating,
                        UserId = r.UserId,
                        UserName = user != null ? user.FirstName + " " + user.LastName : null,
                        UserImg = user != null ? user.Img : null
                    };
                }).ToList()
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




        public async Task<object> GetAllServicesInProgress(string userId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return new { ProviderServices = new List<ProviderServices>(), RequestServices = new List<RequestService>() };

            var services = await _context.providerServices
                .Include(c => c.serviceProvider)
                .Include(c => c.ServicesImages)
                .Include(c => c.offerSalaries)
                .Where(c => c.serviceProviderId == user.Id && c.ServiceStatus == ServiceStatus.Paid)
                .ToListAsync();

            var serviceDtos = services.Select(item => new ProviderServices
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ServiceRequestTime = item.ServiceRequestTime,
                Price = item.Price,
                PriceDiscount = item.PriceDiscount,
                Deliverytime = item.Deliverytime,
                Notes = item.Notes,
                categoryId = item.categoryId,
                serviceProviderId = item.serviceProviderId,
                ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                providerImg = item.serviceProvider.Img,
                Images = item.ServicesImages?.Select(img => img.Img).ToList() ?? new List<string>(),
                video = item.video,
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                CountOfOffers = item.offerSalaries?.Count ?? 0
            });

            var requests = await _context.requestServices
                .Include(c => c.UserProfile)
                .Include(c => c.requestServiceImages)
                .Include(c => c.offerSalaries)
                .Where(c =>c.providerId== user.UserId && c.ServiceStatus == ServiceStatus.Paid)
                .ToListAsync();

            var requestserviceDtos = requests.Select(item => new RequestService
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
                video=item.video,
                offerSalaries = item.offerSalaries?.ToList() ?? new List<OfferSalary>(),
                OffersCount= item.offerSalaries?.Count ?? 0
            }).ToList();

            return new
            {
                ProviderServices = serviceDtos,
                RequestServices =requestserviceDtos
            };
        }

        public async Task CompleteAsync(string serviceId,string userId)
        {
            var service = await _context.providerServices
                .Include(c => c.serviceProvider)
                .Include(g => g.ServicesImages)
                .Include(g => g.offerSalaries)
                .FirstOrDefaultAsync(g => g.Id == serviceId &&g.uId==userId && g.ServiceStatus==ServiceStatus.Paid);
            var provviderr = await _context.serviceProviders.FirstOrDefaultAsync(p => p.UserId == userId);
            var payment = await _context.payments.FirstOrDefaultAsync(p => p.UserId == userId);
            var user = await _context.users.FirstOrDefaultAsync(u => u.Id == payment.UserId);
            if (service != null)
            {
                service.ServiceStatus = ServiceStatus.Completed;
                
                string title = "تم تنفيذ الخدمة";
                string body = $"تم تنفيذ خدمتك {service.Name} من قبل موفر الخدمة {provviderr.FirstName} {provviderr.LastName}، برجاء تأكيد الاستلام.";

               

                if (service?.Id != null)
                {
                    await _firebase.SendNotificationAsync(
                        user.FcmToken,
                        title,
                        body
                    );

                    _context.notifications.Add(new Notifications
                    {
                        UserId = provviderr.UserId,
                        Title = title,
                        Body = body,
                        userImg = provviderr.Img,
                        serviceId = service.Id,
                        CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                    });
                }
                var images = service.ServicesImages?.Select(si => si.Img).ToList() ?? new List<string>();

                var gallery = new Servicesgallery
                {
                    galleryName = service.Name,
                    Description = service.Description,
                    Deliverytime = service.Deliverytime,
                    video = service.video, 
                    serviceProviderId = service.serviceProviderId,
                    serviceProvider = service.serviceProvider,
                    Images = images
                };

                gallery.galleryImages = images
                    .Select(img => new ServicesgalleryImage
                    {
                        Img = img,
                        galleryId = gallery.Id
                    })
                    .ToList();



                provviderr.servicesgalleries.Add(gallery);

                await _context.SaveChangesAsync();


            }
            else
            {
                var request = await _context.requestServices
                   .Include(g => g.requestServiceImages)
                   .Include(g => g.UserProfile)
                   .Include(g => g.offerSalaries)
                   .FirstOrDefaultAsync(g => g.Id == serviceId && g.providerId==userId && g.ServiceStatus == ServiceStatus.Paid);
                if (request != null)
                {
                  request.ServiceStatus = ServiceStatus.Completed;
                    string title = "تم تنفيذ الخدمة";
                    string body = $"تم تنفيذ خدمتك {request.Name} من قبل موفر الخدمة {provviderr.FirstName} {provviderr.LastName}، برجاء تأكيد الاستلام.";

                    if (request?.Id != null)
                    {
                        await _firebase.SendNotificationAsync(
                            user.FcmToken,
                            title,
                            body
                        );

                        _context.notifications.Add(new Notifications
                        {
                            UserId = provviderr.UserId,
                            Title = title,
                            Body = body,
                            userImg = provviderr.Img,
                            serviceId = request.Id,
                            CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                        });
                    }
                    var imagess = request.requestServiceImages?.Select(si => si.Img).ToList() ?? new List<string>();

                    var gallery = new Servicesgallery
                    {
                        galleryName = request.Name,
                        Description = request.Notes,
                        Deliverytime = request.Deliverytime,
                        video=request.video,
                        serviceProviderId = request.providerId,
                        Images = imagess
                    };

                    gallery.galleryImages = imagess
                        .Select(img => new ServicesgalleryImage
                        {
                            Img = img,
                            galleryId = gallery.Id
                        })
                        .ToList();

                    provviderr.servicesgalleries.Add(gallery);

                    await _context.SaveChangesAsync();
                }
            }
            await _context.SaveChangesAsync();
        }

    }
}
