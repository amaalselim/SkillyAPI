using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IProviderServicesRepository
    {
        Task<IEnumerable<ProviderServices>> GetAllProviderService(string currentUserId,double? userLat = null, double? userLng = null);
        Task<IEnumerable<ProviderServices>> GetSortedProviderServicesAsync(
    string sortBy, double? userLat = null, double? userLon = null, string currentUserId=null);
        Task<IEnumerable<ProviderServices>> GetAllServicesByproviderId(string userId);
        Task<ProviderServices> GetProviderServiceByIdAsync(string galleryId, string currentUserId);
        Task AddProviderService(ProviderservicesDTO providerservicesDTO, string userId);
        Task EditProviderService(ProviderservicesDTO providerservicesDTO, string userId, string serviceId);
        Task DeleteProviderServiceAsync(string serviceId, string userId);
        Task<List<ProviderServices>> GetAllservicesbyCategoryId(string currentUserId,string categoryId, string sortBy, double? userLat = null, double? userLon = null);
        Task<IEnumerable<ProviderServices>> GetAllProviderServiceDiscounted(double? userLat = null, double? userLng = null);
        Task UseServiceDiscount(string serviceId, string userId);
        Task<object> GetAllServicesInProgress(string userId);
        Task CompleteAsync(string serviceId,string userId);
    }
}
