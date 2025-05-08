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
        Task<IEnumerable<ProviderServices>> GetAllProviderService();
        Task<IEnumerable<ProviderServices>> GetSortedProviderServicesAsync(
    string sortBy, double? userLat = null, double? userLon = null);
        Task<IEnumerable<ProviderServices>> GetAllServicesByproviderId(string userId);
        Task<ProviderServices> GetProviderServiceByIdAsync(string galleryId);
        Task AddProviderService(ProviderservicesDTO providerservicesDTO, string userId);
        Task EditProviderService(ProviderservicesDTO providerservicesDTO, string userId, string serviceId);
        Task DeleteProviderServiceAsync(string serviceId, string userId);
        Task<List<ProviderServices>> GetAllservicesbyCategoryId(string categoryId);
    }
}
