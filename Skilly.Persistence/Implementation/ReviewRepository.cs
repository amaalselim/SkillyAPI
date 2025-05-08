using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.DTOs;
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
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ReviewRepository(ApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task AddReviewproviderAsync(string userId, ReviewDTO reviewDTO)
        {
            var review = _mapper.Map<Review>(reviewDTO);
            review.UserId = userId;
            review.UserName = _context.users.Where(u => u.Id == userId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault();
            review.ProviderId = reviewDTO.providerId;
            await _context.reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task AddReviewserviceAsync(string userId, ReviewServiceDTO reviewDTO)
        {
            var review = _mapper.Map<Review>(reviewDTO);
            review.UserId = userId;
            review.UserName = _context.users.Where(u => u.Id == userId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault();
            review.serviceId= reviewDTO.serviceId;
            await _context.reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ReviewDisplayDTO>> GetAllReviewsByProviderIdAsync(string providerId)
        {
            var reviews = await _context.reviews
                .Where(r => r.ServiceProvider.UserId == providerId)
                .ToListAsync();

            var userIds = reviews.Select(r => r.UserId).Distinct().ToList(); 
            var users = await _context.userProfiles
                .Where(u => userIds.Contains(u.UserId))
                .ToListAsync();

         
            var reviewDisplayDTOs = reviews.Select(r => new ReviewDisplayDTO
            {
                providerId = providerId, 
                userName = users.FirstOrDefault(u => u.UserId == r.UserId)?.FirstName+' '+
                users.FirstOrDefault(u => u.UserId == r.UserId)?.LastName, 
                userImage = users.FirstOrDefault(u => u.UserId == r.UserId)?.Img,
                Feedback = r.Feedback, 
                Rating = r.Rating     
            }).ToList();

            return reviewDisplayDTOs;
        }

        public async Task<IEnumerable<ReviewserviceDisplayDTO>> GetAllReviewsByserviceIdAsync(string serviceId)
        {
            var reviews = await _context.reviews
                .Where(r => r.serviceId ==serviceId)
                .ToListAsync();

            var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
            var users = await _context.userProfiles
                .Where(u => userIds.Contains(u.UserId))
                .ToListAsync();


            var reviewDisplayDTOs = reviews.Select(r => new ReviewserviceDisplayDTO
            {
               serviceId = serviceId,
                userName = users.FirstOrDefault(u => u.UserId == r.UserId)?.FirstName + ' ' +
                users.FirstOrDefault(u => u.UserId == r.UserId)?.LastName,
                userImage = users.FirstOrDefault(u => u.UserId == r.UserId)?.Img,
                Feedback = r.Feedback,
                Rating = r.Rating
            }).ToList();

            return reviewDisplayDTOs;
        }




        public async Task<decimal> GetAverageRatingByProviderIdAsync(string providerId)
        {
            var reviews = await _context.reviews
                .Where(r => r.ServiceProvider.UserId == providerId)
                .ToListAsync();

            if (reviews == null || !reviews.Any())
                return 0;

            return Math.Round(reviews.Average(r => r.Rating), 2);
        }

        public async Task<decimal> GetAverageRatingByserviceIdAsync(string serviceId)
        {
            var reviews = await _context.reviews
                .Where(r => r.serviceId == serviceId)
                .ToListAsync();

            if (reviews == null || !reviews.Any())
                return 0;

            return Math.Round(reviews.Average(r => r.Rating), 2);
        }

    }
}
