
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IServiceProviderRepository 
    {
        Task<List<ServiceProvider>> GetAllServiceProviderAsync();
        Task<ServiceProvider> GetByIdAsync(string id);
        Task AddServiceProviderAsync(ServiceProviderDTO serviceProviderDTO, string userId);
        Task EditServiceProviderAsync(editServiceproviderDTO serviceProviderDTO, string userId);
        Task<ServiceProvider> GetproviderByuserIdAsync(string id);
        Task DeleteServiceProviderAsync(string Id);
        Task<List<ServiceProvider>> GetAllserviceProvidersbyCategoryId(string categoryId);
        Task<ServiceProvider> GetproviderByIdAsync(string id);
    }
}
