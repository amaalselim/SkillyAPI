using Skilly.Application.DTOs.Review;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IReviewRepository
    {
        Task AddReviewserviceAsync(string userId, ReviewServiceDTO reviewDTO);
        Task<ReviewsWithAverageDTO> GetAllReviews();
        Task<ReviewsWithAverageDTO> GetAllReviewsByproviderIdAsync(string providerId);
        Task<ReviewsWithAverageDTO> GetAllReviewsByserviceIdAsync(string serviceId);
    }
}
