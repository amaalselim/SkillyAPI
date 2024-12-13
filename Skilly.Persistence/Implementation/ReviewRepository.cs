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
        public async Task AddReviewAsync(string userId, ReviewDTO reviewDTO)
        {
            var User= await _context.users.FindAsync(userId);
           
            reviewDTO.UserId = userId;
            reviewDTO.Username = User.FirstName+' '+User.LastName;
        
            
            var review = _mapper.Map<Review>(reviewDTO);
            await _context.reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ReviewDTO>> GetAllReviewsByProviderIdAsync(string ProviderId)
        {

            var review= await _context.reviews.Where(p=>p.ProviderId==ProviderId)
                .ToListAsync();
            if (review == null || !review.Any())
            {
                return new List<ReviewDTO>();
            }
            return _mapper.Map<List<ReviewDTO>>(review);    
        }
    }
}
