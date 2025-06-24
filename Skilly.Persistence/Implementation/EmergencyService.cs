using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.DTOs;
using Skilly.Application.DTOs.Emergency;
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
    public class EmergencyService : IEmergencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EmergencyService(ApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<string> CreateEmergencyRequestAsync(EmergencyRequestDTO emergencyRequestDTO, string userId)
        {
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId== userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            var request=_mapper.Map<EmergencyRequest>(emergencyRequestDTO);
            request.UserId = userId;
            request.RequestTime= DateOnly.FromDateTime(DateTime.Now);
            request.Status = "Pending";
            await _context.emergencyRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return request.Id; 
        }

        public async Task<List<nearestprovidersDTO>> GetNearbyProvidersAsync(string requestId)
        {
            var request = await _context.emergencyRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            var providers = await _context.serviceProviders
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p=>p.Reviews)
                .Where(p => p.categoryId == request.CategoryId && p.IsEmergency)
                .ToListAsync();

            var user = await _context.users
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (user == null || user.Latitude == null || user.Longitude == null)
                return new List<nearestprovidersDTO>();

            var sortedProviders = providers
                .Where(p => p.User.Latitude != null && p.User.Longitude != null)
                .Select(p =>
                {
                    double distance = GeoHelper.GetDistance(
                        p.User.Latitude, p.User.Longitude,
                        user.Latitude, user.Longitude
                    ) ?? double.MaxValue;
                    double estimatedSeconds = (distance / 0.0111);
                    TimeSpan time = TimeSpan.FromSeconds(estimatedSeconds);

                    string estimatedTimeFormatted = $" خلال {(int)time.Hours} ساعة {(int)time.Minutes} دقيقة {(int)time.Seconds} ثانية";

                    return new nearestprovidersDTO
                    {
                        Id = p.UserId,
                        requestId=requestId,
                        Name = $"{p.User.FirstName} {p.User.LastName}",
                        ImageUrl = p.Img,
                        CategoryName = p.Category.ProfessionName,
                        Price = p.PricePerEmergencyService,
                        DistanceInKm = Math.Round(distance, 2),
                        EstimatedTimeFormatted = estimatedTimeFormatted,
                        Review = p.Reviews.Any() ?
                            $"{p.Reviews.Average(r => r.Rating):0.0}":
                            "0"
                    };
                })
                .OrderBy(p => p.DistanceInKm)
                .ToList();

            return sortedProviders;
        }
        public async Task SendEmergencyToDashboardbyId(string emergencyId,decimal price)
        {
            var emergency = await _context.emergencyRequests
                .Include(c=>c.AssignedProvider)
                .FirstOrDefaultAsync(c=>c.Id==emergencyId);

            if (emergency == null)
            {
                throw new Exception("Emergency request not found.");
            }
            var providers = await _context.serviceProviders
                .Where(c => c.IsEmergency && c.categoryId == emergency.CategoryId)
                .ToListAsync();

            foreach (var provider in providers)
            {
                provider.PricePerEmergencyService = price;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<EmergencyRequest>> GetAllEmergencyRequestsAsync()
        {
            var emergency= await _context.emergencyRequests
                .Include(p=>p.User)
                .Include(p=>p.Category)
                .Include(p => p.AssignedProvider)
                .ToListAsync();

            var emer = emergency.Select(r => new EmergencyRequest
            {
                Id=r.Id,
                UserId = r.UserId,
                UserName = $"{r.User.FirstName} {r.User.LastName}",
                CategoryId = r.CategoryId,
                CategoryName = r.Category.Name,
                ProblemDescription = r.ProblemDescription,
                RequestTime = r.RequestTime,
                Status = r.Status,
                AssignedProviderId = r.AssignedProviderId,
                AssignedProviderName = r.AssignedProvider != null ? $"{r.AssignedProvider.FirstName} {r.AssignedProvider.LastName}" : null,
                Finalprice = r.Finalprice
            }).ToList();
            return emer;
        }

        public async Task<EmergencyRequest> GetEmergencyRequestByIdAsync(string requestId)
        {
            var emergency = await _context.emergencyRequests
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.AssignedProvider)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (emergency == null)
                return null;

            var emer = new EmergencyRequest
            {
                Id = emergency.Id,
                UserId = emergency.UserId,
                UserName = $"{emergency.User.FirstName} {emergency.User.LastName}",
                CategoryId = emergency.CategoryId,
                CategoryName = emergency.Category.Name,
                ProblemDescription = emergency.ProblemDescription,
                RequestTime = emergency.RequestTime,
                Status = emergency.Status,
                AssignedProviderId = emergency.AssignedProviderId,
                AssignedProviderName = emergency.AssignedProvider != null
                    ? $"{emergency.AssignedProvider.FirstName} {emergency.AssignedProvider.LastName}"
                    : null,
                Finalprice = emergency.Finalprice
            };

            return emer;
        }

        public async Task<bool> AcceptEmergencyOfferAsync(string providerId, string requestId)
        {
            
            var provider = await _context.serviceProviders
                .FirstOrDefaultAsync(p => p.UserId == providerId);

            if (provider == null)
                return false;

            var request = await _context.emergencyRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return false;

            request.AssignedProviderId = provider.UserId;
            request.Finalprice = provider.PricePerEmergencyService;
            request.Status = "Accepted";

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectEmergencyOfferAsync(string providerId, string requestId)
        {
            var provider = await _context.serviceProviders
                .FirstOrDefaultAsync(p => p.UserId == providerId);

            if (provider == null)
                return false;

            var request = await _context.emergencyRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return false;

            request.Status= "Rejected";

            await _context.SaveChangesAsync();

            return true;
        }




    }
}
