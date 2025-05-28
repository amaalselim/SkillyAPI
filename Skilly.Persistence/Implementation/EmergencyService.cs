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
        public async Task CreateEmergencyRequestAsync(EmergencyRequestDTO emergencyRequestDTO, string userId)
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

            //send notification to admin that a new emergency request has been created


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
        public async Task<List<EmergencyRequest>> GetAllEmergencyRequestsAsync()
        {
            return await _context.emergencyRequests
                .ToListAsync();
        }
        public async Task<EmergencyRequest> GetEmergencyRequestByIdAsync(string requestId)
        {
            return await _context.emergencyRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);
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
