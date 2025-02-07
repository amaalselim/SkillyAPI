using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IUserProfileRepository ProfileRepository { get; }
        IServiceProviderRepository ServiceProviderRepository { get; }
        IServicegalleryRepository servicegalleryRepository { get; }
        IProviderServicesRepository providerServiceRepository { get; }
        IReviewRepository reviewRepository { get; }
        ICategoryRepository _categoryRepository { get; }
        IRequestserviceRepository _requestserviceRepository { get; }
        Task<int> CompleteAsync();
    }
}
