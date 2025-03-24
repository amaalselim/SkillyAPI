using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using Skilly.Persistence.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IOfferSalaryRepository
    {
        Task<List<offersalaryDTO>> GetAllOffersAsync();
        Task<List<offersalaryDTO>> GetAllOffersByServiceId(string serviceId);
        Task<offersalaryDTO> GetOfferByIdAsync(string id);
        Task<offersalaryDTO> GetOfferByserviceIdAsync(string serviceId);
        Task AddOfferAsync(offersalaryDTO offersalaryDTO);
        Task UpdateOfferAsync(offersalaryDTO offersalaryDTO, string id);
        Task DeleteOfferAsync(string id);
        Task<int> GetOffersCountByServiceIdAsync(string serviceId);

    }
}
