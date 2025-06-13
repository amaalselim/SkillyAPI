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

            var reviews = await _context.reviews
                .ToListAsync();

            var serviceDtos = services.Select(item =>
            {
                var relatedReviews = reviews.Where(r => r.ProviderId== item.serviceProviderId).ToList();
                var avgRate = relatedReviews.Any() ? Math.Round(relatedReviews.Average(r => r.Rating), 2) : 0;

                return new ProviderServices
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
                    AverageRating = avgRate,
                    Images = item.ServicesImages?.Select(img => new ProviderServicesImage
                    {
                        Id = img.Id,
                        Img = img.Img
                    }).ToList() ?? new List<ProviderServicesImage>(),
                };
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
            //var service = _mapper.Map<ProviderServices>(providerservicesDTO);
            var service = new ProviderServices
            {
                Name = providerservicesDTO.Name,
                Description = providerservicesDTO.Description,
                Price = providerservicesDTO.Price,
                Deliverytime = providerservicesDTO.Deliverytime,
                Notes = providerservicesDTO.Notes

            };
            service.serviceProviderId = user.Id;
            service.ServiceRequestTime = DateOnly.FromDateTime(DateTime.Now);
            service.providerImg = user.Img;
            service.uId = user.UserId;
            service.categoryId = user.categoryId; 
            service.ServiceStatus = ServiceStatus.Posted;
            if (providerservicesDTO.video != null)
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
            var user = await _context.serviceProviders
                .FirstOrDefaultAsync(u => u.UserId == userId);

            var service = await _context.providerServices
                .Include(s => s.ServicesImages)
                .FirstOrDefaultAsync(s => s.Id == serviceId && s.uId == user.UserId);

            if (service == null)
                throw new ProviderServiceNotFoundException("Service not found.");

  
            var payments = await _context.payments
                .Where(p => p.ProviderServiceId== serviceId)
                .ToListAsync();
            _context.payments.RemoveRange(payments);

       
            var reviews = await _context.reviews
                .Where(r => r.serviceId == serviceId)
                .ToListAsync();
            _context.reviews.RemoveRange(reviews);

            var bookings = await _context.offerSalaries
                .Where(b => b.serviceId== serviceId)
                .ToListAsync();
            _context.offerSalaries.RemoveRange(bookings);

            var not = await _context.notifications
                .Where(b => b.serviceId == serviceId)
                .ToListAsync();
            _context.notifications.RemoveRange(not);

            _context.providerServices.Remove(service);

            await _context.SaveChangesAsync();
        }

        public async Task EditProviderService(EditProviderServiceDTO providerservicesDTO, string userId, string serviceId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new ServiceProviderNotFoundException("Service Provider not found.");
            }

            var service = await _context.providerServices
                .Include(g => g.ServicesImages)
                .FirstOrDefaultAsync(g => g.Id == serviceId && g.uId == user.UserId);

            if (service == null)
            {
                throw new ProviderServiceNotFoundException("Provider service not found.");
            }

            if (providerservicesDTO.ImagesToDeleteIds != null && providerservicesDTO.ImagesToDeleteIds.Any())
            {
                var imagesToDelete = service.ServicesImages
                     .Where(img => providerservicesDTO.ImagesToDeleteIds.Contains(img.Id))
                     .ToList();

                foreach (var img in imagesToDelete)
                {
                    await _imageService.DeleteFileAsync(img.Img);
                    service.ServicesImages.Remove(img);
                }
                await _context.SaveChangesAsync();
            }


            service.Name = providerservicesDTO.Name;
            service.Description = providerservicesDTO.Description;
            service.Price = providerservicesDTO.Price;
            service.Deliverytime = providerservicesDTO.Deliverytime;
            service.Notes = providerservicesDTO.Notes;
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

                if (!string.IsNullOrEmpty(service.video))
                {
                    await _imageService.DeleteFileAsync(service.video);
                }
                await _context.SaveChangesAsync();

                var videoPath = await _imageService.SaveFileAsync(providerservicesDTO.video, path);
                service.video = videoPath;
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

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProviderServices>> GetAllProviderService()
        {
            var services = await _context.providerServices
                .Include(i => i.ServicesImages)
                .Include(i => i.serviceProvider)
                .ThenInclude(sp => sp.User)
                .Include(i => i.serviceProvider)
                .ThenInclude(i=>i.Reviews)
                .Include(i => i.serviceProvider)
                .Include(i => i.Category)
                .Include(i => i.offerSalaries)
                .Include(i => i.Reviews)
                .ToListAsync();

            if (services == null || !services.Any())
                return new List<ProviderServices>();
            

            var allUserIds = services.SelectMany(s => s.Reviews.Select(r => r.UserId)).Distinct().ToList();
            var allUsers = await _context.userProfiles
                .Where(u => allUserIds.Contains(u.UserId))
                .ToListAsync();


            var reviews = await _context.reviews
                .ToListAsync();

            var serviceDtos = services.Select(item =>
            {

                //var providers = _context.serviceProviders.Include(p => p.Reviews)
                //.FirstOrDefault(p=>p.Equals(item.serviceProviderId));

 
                var relatedReviews = reviews.Where(r => r.ProviderId == item.serviceProviderId).ToList();
                var avgRate = relatedReviews.Any() ? Math.Round(relatedReviews.Average(r => r.Rating), 2) : 0;

                    return new ProviderServices
                    {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    ServiceRequestTime = item.ServiceRequestTime,
                    Price = item.Price,
                    PriceDiscount = item.PriceDiscount,
                    AverageRating = avgRate,
                    Deliverytime = item.Deliverytime,
                    categoryId = item.categoryId,
                    CategoryName = item.Category != null ? item.Category.Name : null,
                    Notes = item.Notes,
                    ServiceStatus = item.ServiceStatus,
                    serviceProviderId = item.serviceProviderId,
                    ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                    providerImg = item.serviceProvider.Img,

                    Images = item.ServicesImages?.Select(img => new ProviderServicesImage
                    {
                        Id = img.Id,
                        Img = img.Img
                    }).ToList() ?? new List<ProviderServicesImage>(),
                    video = item.video,

                    offerSalaries = item.offerSalaries.Where(p => p.Status == 0)?.ToList() ?? new List<OfferSalary>(),
                    CountOfOffers = item.offerSalaries?.Count(p => p.Status == 0) ?? 0,

                    

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
                };
            }).ToList();

            
            return serviceDtos;
        }
        public async Task<ProviderServices> GetProviderServiceByIdAsync(string currentUserId,string serviceId)
        {
            var service = await _context.providerServices
                .Include(c => c.serviceProvider)
                .Include(g => g.ServicesImages)
                .Include(g => g.offerSalaries)
                .Include(g => g.Category)
                .FirstOrDefaultAsync(g => g.Id == serviceId);


            var provider = await _context.serviceProviders
                .FirstOrDefaultAsync(c => c.UserId == service.uId);

            var reviews = await _context.reviews
            .ToListAsync();

            var relatedReviews = reviews.Where(r => r.ProviderId == service.serviceProviderId).ToList();
            var avgRate = relatedReviews.Any() ? Math.Round(relatedReviews.Average(r => r.Rating), 2) : 0;

            if (service == null)
                throw new ProviderServiceNotFoundException("Service not found.");
            var acceptedOffer =service.offerSalaries
                    .FirstOrDefault(o => o.Status == OfferStatus.Accepted && o.userId == currentUserId);


            var serviceDto = new ProviderServices
            {

                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                ServiceRequestTime = service.ServiceRequestTime,
                Price = acceptedOffer != null ? acceptedOffer.Salary : service.Price,
                PriceDiscount = service.PriceDiscount,
                Deliverytime = acceptedOffer != null ? acceptedOffer.Deliverytime : service.Deliverytime,
                categoryId = service.categoryId,
                CategoryName = service.Category.Name,
                Notes = service.Notes,
                ServiceStatus = service.ServiceStatus,
                serviceProviderId = service.serviceProviderId,
                ServiceProviderName = service.serviceProvider.FirstName + " " + service.serviceProvider.LastName,
                providerImg = service.serviceProvider.Img,
                Images = service.ServicesImages?.Select(img => new ProviderServicesImage
                {
                    Id = img.Id,
                    Img = img.Img
                }).ToList() ?? new List<ProviderServicesImage>(),
                video = service.video,
                AverageRating = avgRate,
                offerSalaries = service.offerSalaries.Where(p => p.Status == 0)?.ToList() ?? new List<OfferSalary>(),
                CountOfOffers = service.offerSalaries?.Count(p => p.Status == 0) ?? 0,
            };

            return serviceDto;
        }

        public async Task<List<ProviderServices>> GetAllservicesbyCategoryId(string currentUserId,string categoryId, string sortBy, double? userLat = null, double? userLon = null)
        {
            var services = await _context.providerServices
                .Include(i => i.ServicesImages)
                .Include(i => i.serviceProvider)
                    .ThenInclude(sp => sp.User)
                .Include(i => i.offerSalaries)
                .Include(i => i.Reviews)
                .Include(i => i.Category)
                .Where(c => c.categoryId == categoryId)
                .ToListAsync();

            if (services == null || !services.Any())
                return new List<ProviderServices>();

            var reviews = await _context.reviews
                .ToListAsync();

            var serviceDtos = services.Select(item =>
            {
                var acceptedOffer = item.offerSalaries
                    .FirstOrDefault(o => o.Status == OfferStatus.Accepted && o.userId == currentUserId);
                var relatedReviews = reviews.Where(r => r.ProviderId == item.serviceProviderId).ToList();
                var avgRate = relatedReviews.Any() ? Math.Round(relatedReviews.Average(r => r.Rating), 2) : 0;
                return new ProviderServices
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    ServiceRequestTime = item.ServiceRequestTime,
                    Price = acceptedOffer != null ? acceptedOffer.Salary : item.Price,
                    PriceDiscount = item.PriceDiscount,
                    Deliverytime = acceptedOffer != null ? acceptedOffer.Deliverytime : item.Deliverytime,
                    Notes = item.Notes,
                    categoryId = item.categoryId,
                    CategoryName = item.Category != null ? item.Category.Name : null,
                    ServiceStatus = item.ServiceStatus,
                    serviceProviderId = item.serviceProviderId,
                    ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                    providerImg = item.serviceProvider.Img,
                    AverageRating = avgRate,
                    Images = item.ServicesImages?.Select(img => new ProviderServicesImage
                    {
                        Id = img.Id,
                        Img = img.Img
                    }).ToList() ?? new List<ProviderServicesImage>(),
                    video = item.video,
                    offerSalaries = item.offerSalaries.Where(p => p.Status == 0)?.ToList() ?? new List<OfferSalary>(),
                    CountOfOffers = item.offerSalaries?.Count(p => p.Status == 0) ?? 0,
                    Distance = (userLat != null && userLon != null && item.serviceProvider?.User != null)
                        ? GeoHelper.GetDistance(userLat.Value, userLon.Value, item.serviceProvider.User.Latitude, item.serviceProvider.User.Longitude).GetValueOrDefault()
                        : 0,
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


        public async Task<IEnumerable<ProviderServices>> GetAllServicesByproviderId(string userId)
        {
            var user = await _context.serviceProviders.FirstOrDefaultAsync(u => u.UserId == userId || u.Id==userId);
            if (user == null)
                return new List<ProviderServices>();


            var services = await _context.providerServices
                .Include(c => c.serviceProvider)
                .Include(c => c.ServicesImages)
                .Include(c => c.offerSalaries)
                .Include(c => c.Category)
                .Where(c => c.serviceProviderId == user.Id)
                .ToListAsync();

            
            if (services == null || !services.Any())
                return new List<ProviderServices>();

            var reviews = await _context.reviews
                .ToListAsync();


            var serviceDtos = services.Select(item =>
            {
                var acceptedOffer = item.offerSalaries
                    .FirstOrDefault(o => o.Status == OfferStatus.Accepted && o.userId == userId);
                var relatedReviews = reviews.Where(r => r.ProviderId == item.serviceProviderId).ToList();
                var avgRate = relatedReviews.Any() ? Math.Round(relatedReviews.Average(r => r.Rating), 2) : 0;
                return new ProviderServices
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    ServiceRequestTime = item.ServiceRequestTime,
                    Price = acceptedOffer != null ? acceptedOffer.Salary : item.Price,
                    PriceDiscount = item.PriceDiscount,
                    Deliverytime = acceptedOffer != null ? acceptedOffer.Deliverytime : item.Deliverytime,
                    Notes = item.Notes,
                    categoryId = item.categoryId,
                    CategoryName = item.Category != null ? item.Category.Name : null,
                    ServiceStatus = item.ServiceStatus,
                    serviceProviderId = item.serviceProviderId,
                    ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                    providerImg = item.serviceProvider.Img,
                    AverageRating = avgRate,
                    Images = item.ServicesImages?.Select(img => new ProviderServicesImage
                    {
                        Id = img.Id,
                        Img = img.Img
                    }).ToList() ?? new List<ProviderServicesImage>(),
                    video = item.video,
                    offerSalaries = item.offerSalaries?.Where(p => p.Status == 0).ToList() ?? new List<OfferSalary>(),
                    CountOfOffers = item.offerSalaries?.Count(p => p.Status == 0) ?? 0,
                };
            }).ToList();

            return serviceDtos;

        }

        public async Task<IEnumerable<ProviderServices>> GetSortedProviderServicesAsync(
            string sortBy, double? userLat = null, double? userLon = null, string currentUserId=null)
        {
            var services = await _context.providerServices
                .Include(s => s.serviceProvider)
                    .ThenInclude(sp => sp.User)
                .Include(s => s.ServicesImages)
                .Include(s => s.offerSalaries)
                .Include(s=>s.Category)
                .Include(s => s.Reviews)
                .ToListAsync();

            if (services == null || !services.Any())
                return new List<ProviderServices>();

            var allUserIds = services.SelectMany(s => s.Reviews.Select(r => r.UserId)).Distinct().ToList();
            var allUsers = await _context.userProfiles
                .Where(u => allUserIds.Contains(u.UserId))
                .ToListAsync();

            var reviews = await _context.reviews.ToListAsync();

            var serviceDtos = services.Select(item =>
            {
                var acceptedOffer = !string.IsNullOrEmpty(currentUserId)
                    ? item.offerSalaries.FirstOrDefault(o => o.Status == OfferStatus.Accepted && o.userId == currentUserId)
                    : null;

                var relatedReviews = reviews.Where(r => r.ProviderId == item.serviceProviderId).ToList();
                var avgRate = relatedReviews.Any() ? Math.Round(relatedReviews.Average(r => r.Rating), 2) : 0;

                return new ProviderServices
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    ServiceRequestTime = item.ServiceRequestTime,
                    Price = acceptedOffer != null ? acceptedOffer.Salary : item.Price,
                    PriceDiscount = item.PriceDiscount,
                    Deliverytime = acceptedOffer != null ? acceptedOffer.Deliverytime : item.Deliverytime,
                    Notes = item.Notes,
                    categoryId = item.categoryId,
                    CategoryName = item.Category != null ? item.Category.Name : null,
                    ServiceStatus = item.ServiceStatus,
                    serviceProviderId = item.serviceProviderId,
                    ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                    providerImg = item.serviceProvider.Img,
                    AverageRating=avgRate,
                    Images = item.ServicesImages?.Select(img => new ProviderServicesImage
                    {
                        Id = img.Id,
                        Img= img.Img
                    }).ToList() ?? new List<ProviderServicesImage>(),

                    video = item.video,
                    offerSalaries = item.offerSalaries.Where(p => p.Status == 0).ToList() ?? new List<OfferSalary>(),
                    CountOfOffers = item.offerSalaries?.Count(p => p.Status == 0) ?? 0,
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
                };
            }).ToList();

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "price-asc":
                        serviceDtos = serviceDtos.OrderBy(s => s.Price).ToList();
                        break;
                    case "latest":
                        serviceDtos = serviceDtos.OrderByDescending(s => s.ServiceRequestTime).ToList();
                        break;
                    case "nearest":
                        if (userLat != null && userLon != null)
                            serviceDtos = serviceDtos.OrderBy(s => s.Distance).ToList();
                        break;
                    default:
                        serviceDtos = serviceDtos.OrderBy(s => s.Price).ToList();
                        break;
                }
            }
            else
            {

                if (userLat != null && userLon != null)
                    serviceDtos = serviceDtos.OrderBy(s => s.Distance).ToList();
            }

            return serviceDtos;
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
                .Include(c => c.Category)
                .Where(c => c.serviceProviderId == user.Id && c.ServiceStatus == ServiceStatus.Paid)
                .ToListAsync();

            var reviews = await _context.reviews
                .ToListAsync();

            var serviceDtos = services.Select(item =>
            {
                
                var acceptedOffer = item.offerSalaries
                    .FirstOrDefault(o => o.Status == OfferStatus.Accepted);

                var relatedReviews = reviews.Where(r => r.ProviderId == item.serviceProviderId).ToList();
                var avgRate = relatedReviews.Any() ? Math.Round(relatedReviews.Average(r => r.Rating), 2) : 0;

                return new ProviderServices
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    ServiceRequestTime = item.ServiceRequestTime,
                    Price = acceptedOffer != null ? acceptedOffer.Salary : item.Price,
                    PriceDiscount = item.PriceDiscount,
                    Deliverytime= acceptedOffer != null ? acceptedOffer.Deliverytime : item.Deliverytime,
                    Notes = item.Notes,
                    categoryId = item.categoryId,
                    CategoryName = item.Category != null ? item.Category.Name : null,
                    ServiceStatus = item.ServiceStatus,
                    serviceProviderId = item.serviceProviderId,
                    ServiceProviderName = item.serviceProvider.FirstName + " " + item.serviceProvider.LastName,
                    providerImg = item.serviceProvider.Img,
                    Images = item.ServicesImages?.Select(img => new ProviderServicesImage
                    {
                        Id = img.Id,
                        Img = img.Img
                    }).ToList() ?? new List<ProviderServicesImage>(),
                    video = item.video,
                    AverageRating = avgRate,
                    offerSalaries = item.offerSalaries.Where(p => p.Status == 0)?.ToList() ?? new List<OfferSalary>(),
                    CountOfOffers = item.offerSalaries?.Count(p => p.Status == 0) ?? 0
                };
            });

            var requests = await _context.requestServices
                .Include(c => c.UserProfile)
                .Include(c => c.requestServiceImages)
                .Include(c => c.offerSalaries)
                .Where(c => c.providerId == user.UserId && c.ServiceStatus == ServiceStatus.Paid)
                .ToListAsync();

            var requestserviceDtos = requests.Select(item =>
            {
                var acceptedOffer = item.offerSalaries
                    .FirstOrDefault(o => o.Status == OfferStatus.Accepted);

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
                    ServiceStatus = item.ServiceStatus,
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

            return new
            {
                ProviderServices = serviceDtos,
                RequestServices = requestserviceDtos
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
           
            if (service != null)
            {
                var payment = await _context.payments.FirstOrDefaultAsync(p => p.ProviderServiceId == serviceId);
                var user = await _context.users.FirstOrDefaultAsync(u => u.Id == payment.UserId);
                service.ServiceStatus = ServiceStatus.Completed;
                var offers = await _context.offerSalaries.FirstOrDefaultAsync(o => o.serviceId== service.Id && o.Status == OfferStatus.Accepted);

                if (offers != null)
                {
                    offers.Status = OfferStatus.paidandComplete;
                }

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
                        UserId = user.Id,
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
                    Images = images.Select(img => new ServicesgalleryImage
                    {
                        Img = img
                    }).ToList()
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
                    var payment = await _context.payments.FirstOrDefaultAsync(p => p.RequestServiceId == serviceId);
                    var user = await _context.users.FirstOrDefaultAsync(u => u.Id == payment.UserId);
                    request.ServiceStatus = ServiceStatus.Completed;
                    var offers = await _context.offerSalaries.FirstOrDefaultAsync(o => o.requestserviceId == request.Id && o.Status == OfferStatus.Accepted);

                    if (offers != null)
                    {
                        offers.Status = OfferStatus.paidandComplete;
                    }
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
                            UserId = user.Id,
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
                        Images = imagess.Select(img => new ServicesgalleryImage
                        {
                            Img = img
                        }).ToList()
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
