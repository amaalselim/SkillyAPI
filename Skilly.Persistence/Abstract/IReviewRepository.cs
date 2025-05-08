using Skilly.Application.DTOs;
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
        Task AddReviewproviderAsync(string userId, ReviewDTO reviewDTO);
        Task AddReviewserviceAsync(string userId, ReviewServiceDTO reviewDTO);
        Task<IEnumerable<ReviewDisplayDTO>> GetAllReviewsByProviderIdAsync(string providerId);
        Task<IEnumerable<ReviewserviceDisplayDTO>> GetAllReviewsByserviceIdAsync(string serviceId);
        Task<decimal> GetAverageRatingByProviderIdAsync(string providerId);
        Task<decimal> GetAverageRatingByserviceIdAsync(string serviceId);
    }
}
