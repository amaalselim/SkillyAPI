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
        Task AddReviewAsync(string userId,ReviewDTO reviewDTO);
        Task<IEnumerable<ReviewDTO>> GetAllReviewsByProviderIdAsync(string ProviderId);  
    }
}
