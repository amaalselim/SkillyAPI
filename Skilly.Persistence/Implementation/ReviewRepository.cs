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
        public async Task AddReviewAsync(string userId, ReviewDTO reviewDTO)
        {
            var review = _mapper.Map<Review>(reviewDTO);
            review.UserId = userId;
            review.UserName = _context.users.Where(u => u.Id == userId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault();
            review.ProviderId = reviewDTO.providerId;
            await _context.reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ReviewDisplayDTO>> GetAllReviewsByProviderIdAsync(string providerId)
        {
            // جلب جميع UserIds من المراجعات المرتبطة بمزود الخدمة المحدد
            var reviews = await _context.reviews
                .Where(r => r.ServiceProvider.UserId == providerId)
                .ToListAsync();

            // جلب المستخدمين الذين كتبوا هذه المراجعات
            var userIds = reviews.Select(r => r.UserId).Distinct().ToList(); // التأكد من أن معرفات المستخدمين مميزة
            var users = await _context.userProfiles
                .Where(u => userIds.Contains(u.UserId))
                .ToListAsync();

            // إنشاء الـ DTO للمراجعات
            var reviewDisplayDTOs = reviews.Select(r => new ReviewDisplayDTO
            {
                providerId = providerId, // تحديد providerId بشكل صحيح
                userName = users.FirstOrDefault(u => u.UserId == r.UserId)?.FirstName, // ربط اسم المستخدم بـ UserId
                userImage = users.FirstOrDefault(u => u.UserId == r.UserId)?.Img, // ربط صورة المستخدم بـ UserId
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

    }
}
