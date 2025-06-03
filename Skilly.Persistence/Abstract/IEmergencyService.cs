using Skilly.Application.DTOs.Emergency;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IEmergencyService
    {
        Task<string> CreateEmergencyRequestAsync(EmergencyRequestDTO requestDto, string userId);
        Task<List<nearestprovidersDTO>> GetNearbyProvidersAsync(string requestId);
        Task<bool> AcceptEmergencyOfferAsync(string providerId, string requestId);
        Task<bool> RejectEmergencyOfferAsync(string providerId, string requestId);
        Task<List<EmergencyRequest>> GetAllEmergencyRequestsAsync();
        Task<EmergencyRequest> GetEmergencyRequestByIdAsync(string requestId);

    }
}
