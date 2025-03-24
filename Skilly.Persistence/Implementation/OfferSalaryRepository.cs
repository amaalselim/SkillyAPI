using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using Skilly.Persistence.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Implementation
{
    public class OfferSalaryRepository : IOfferSalaryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OfferSalaryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddOfferAsync(offersalaryDTO offersalaryDTO)
        {
            if (!await _context.providerServices.AnyAsync(s => s.Id == offersalaryDTO.serviceId))
            {
                offersalaryDTO.serviceId = null;
            }

            if (!await _context.requestServices.AnyAsync(r => r.Id == offersalaryDTO.requestserviceId))
            {
                offersalaryDTO.requestserviceId = null;
            }

            var offer = _mapper.Map<OfferSalary>(offersalaryDTO);
            await _context.offerSalaries.AddAsync(offer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOfferAsync(string id)
        {
            var offer= await _context.offerSalaries.FindAsync(id);
            _context.offerSalaries.Remove(offer);
            await _context.SaveChangesAsync();
        }

        public async Task<List<offersalaryDTO>> GetAllOffersAsync()
        {
            return await _context.offerSalaries
                .Select(o => new offersalaryDTO
                {
                    ID = o.Id,
                    Salary = o.Salary,
                    Deliverytime = o.Deliverytime,
                    Notes = o.Notes,
                    serviceId = o.ProviderServices != null ? o.ProviderServices.Id : null,
                    requestserviceId = o.RequestService != null ? o.RequestService.Id: null,
                    ServiceName = o.ProviderServices != null ? o.ProviderServices.Name : null,
                    RequestServiceName = o.RequestService != null ? o.RequestService.Name : null
                })
                .ToListAsync();
        }

        public async Task<List<offersalaryDTO>> GetAllOffersByServiceId(string serviceId)
        {
            return await _context.offerSalaries
                .Select(o => new offersalaryDTO
                {
                    ID= o.Id,
                    Salary = o.Salary,
                    Deliverytime = o.Deliverytime,
                    Notes = o.Notes,
                    serviceId = o.ProviderServices != null ? o.ProviderServices.Id : null,
                    requestserviceId = o.RequestService != null ? o.RequestService.Id : null,
                    ServiceName = o.ProviderServices != null ? o.ProviderServices.Name : null,
                    RequestServiceName = o.RequestService != null ? o.RequestService.Name : null
                })
                .Where(x => x.serviceId == serviceId || x.requestserviceId == serviceId)
                .ToListAsync();
        }

        public async Task<offersalaryDTO> GetOfferByIdAsync(string id)
        {
            return await _context.offerSalaries
                .Select(o => new offersalaryDTO
                {
                    ID = o.Id,
                    Salary = o.Salary,
                    Deliverytime = o.Deliverytime,
                    Notes = o.Notes,
                    serviceId = o.ProviderServices != null ? o.ProviderServices.Id : null,
                    requestserviceId = o.RequestService != null ? o.RequestService.Id : null,
                    ServiceName = o.ProviderServices != null ? o.ProviderServices.Name : null,
                    RequestServiceName = o.RequestService != null ? o.RequestService.Name : null
                })
                .FirstOrDefaultAsync(x => x.ID == id);
        }
        public async Task<offersalaryDTO> GetOfferByserviceIdAsync(string serviceId)
        {
            return await _context.offerSalaries
                .Select(o => new offersalaryDTO
                {
                    ID = o.Id,
                    Salary = o.Salary,
                    Deliverytime = o.Deliverytime,
                    Notes = o.Notes,
                    serviceId = o.ProviderServices != null ? o.ProviderServices.Id : null,
                    requestserviceId = o.RequestService != null ? o.RequestService.Id : null,
                    ServiceName = o.ProviderServices != null ? o.ProviderServices.Name : null,
                    RequestServiceName = o.RequestService != null ? o.RequestService.Name : null
                })
                .FirstOrDefaultAsync(x => x.serviceId == serviceId || x.requestserviceId==serviceId);
        }

        public async Task<int> GetOffersCountByServiceIdAsync(string serviceId)
        {
            return await _context.offerSalaries
            .Where(o => o.serviceId == serviceId || o.requestserviceId==serviceId)
            .CountAsync();
        }

        public async Task UpdateOfferAsync(offersalaryDTO offersalaryDTO, string id)
        {
            var offer = await _context.offerSalaries.FindAsync(id);
            _mapper.Map(offersalaryDTO,offer);
            _context.offerSalaries.Update(offer);
            await _context.SaveChangesAsync();
        }
    }
}
