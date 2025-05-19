using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IRequestserviceRepository
    {
        Task<IEnumerable<RequestService>> GetAllRequests(double? userLat = null, double? userLng = null);
        Task<IEnumerable<RequestService>> GetSortedUserAsync(
                string sortBy, double? userLat = null, double? userLon = null);
        Task<IEnumerable<RequestService>> GetAllRequestsByUserId(string userId);
        Task<RequestService> GetRequestById(string requestId);
        Task AddRequestService(requestServiceDTO requestServiceDTO, string userId);
        Task EditRequestService(requestServiceDTO requestServiceDTO, string userId, string requestId);
        Task DeleteRequestServiceAsync(string requestId, string userId);
        Task<IEnumerable<RequestService>> GetAllRequestsByCategoryId(string userId);
    }
}
