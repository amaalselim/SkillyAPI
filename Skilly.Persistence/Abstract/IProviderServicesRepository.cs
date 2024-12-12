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
        Task<ProviderServices> GetProviderServiceByIdAsync(string galleryId, string userId);
        Task AddProviderService(ProviderservicesDTO providerservicesDTO, string userId);
        Task EditProviderService(ProviderservicesDTO providerservicesDTO, string userId, string serviceId);
        Task DeleteProviderServiceAsync(string serviceId, string userId);
    }
}
