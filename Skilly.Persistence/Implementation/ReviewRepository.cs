using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.DTOs;
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
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ReviewRepository(ApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task AddReviewserviceAsync(string userId, ReviewServiceDTO reviewDTO)
        {
            var review = _mapper.Map<Review>(reviewDTO);
            review.UserId = userId;
            review.UserName = _context.users.Where(u => u.Id == userId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault();
            review.serviceId= reviewDTO.serviceId;
            review.UserImg = _context.userProfiles.Where(u => u.UserId == userId).Select(u => u.Img).FirstOrDefault();
            var services = await _context.providerServices
                .Where(s => s.Id == reviewDTO.serviceId)
                .FirstOrDefaultAsync();
            review.ProviderId = services.serviceProviderId;
            await _context.reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<ReviewsWithAverageDTO> GetAllReviewsByproviderIdAsync(string providerId)
        {
            var reviews = await _context.reviews
                .Include(p=>p.ProviderServices)
                .Where(r => r.ProviderServices.uId== providerId)
                .ToListAsync();
            


            var userIds = reviews.Select(r => r.UserId).Distinct().ToList();

            var users = await _context.userProfiles
                .Where(u => userIds.Contains(u.UserId))
                .ToListAsync();

            var reviewDisplayDTOs = reviews.Select(r => new ReviewserviceDisplayDTO
            {
                serviceId=r.ProviderServices.Id,
                serviceName=r.ProviderServices.Name,
                userName = users.FirstOrDefault(u => u.UserId == r.UserId)?.FirstName + " " +
                           users.FirstOrDefault(u => u.UserId == r.UserId)?.LastName,
                userImage = users.FirstOrDefault(u => u.UserId == r.UserId)?.Img,
                Feedback = r.Feedback,
                Rating = r.Rating,
                

            }).ToList();

            var avgRate = Math.Round(reviews.Any() ? reviews.Average(r => r.Rating) : 0, 2);

            return new ReviewsWithAverageDTO
            {
                AverageRating = avgRate,
                Reviews = reviewDisplayDTOs
            };
        }
        public async Task<ReviewsWithAverageDTO> GetAllReviewsByserviceIdAsync(string serviceId)
        {
            var reviews = await _context.reviews
                .Include(p => p.ProviderServices)
                .Where(r => r.serviceId == serviceId)
                .ToListAsync();

            var userIds = reviews.Select(r => r.UserId).Distinct().ToList();

            var users = await _context.userProfiles
                .Where(u => userIds.Contains(u.UserId))
                .ToListAsync();

            var reviewDisplayDTOs = reviews.Select(r => new ReviewserviceDisplayDTO
            {
                serviceId = serviceId,
                serviceName = r.ProviderServices.Name,
                userName = users.FirstOrDefault(u => u.UserId == r.UserId)?.FirstName + " " +
                           users.FirstOrDefault(u => u.UserId == r.UserId)?.LastName,
                userImage = users.FirstOrDefault(u => u.UserId == r.UserId)?.Img,
                Feedback = r.Feedback,
                Rating = r.Rating
            }).ToList();

            var avgRate = Math.Round(reviews.Any() ? reviews.Average(r => r.Rating) : 0, 2);

            return new ReviewsWithAverageDTO
            {
                AverageRating = avgRate,
                Reviews = reviewDisplayDTOs
            };
        }

    }
}
