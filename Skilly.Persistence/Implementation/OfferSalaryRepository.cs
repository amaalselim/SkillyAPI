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
        private readonly FirebaseV1Service _firebase;

        public OfferSalaryRepository(ApplicationDbContext context, IMapper mapper,FirebaseV1Service firebase)
        {
            _context = context;
            _mapper = mapper;
            _firebase = firebase;
        }

        public async Task AddOfferAsync(createofferDTO offersalaryDTO,string userId)
        {
            if (!string.IsNullOrEmpty(offersalaryDTO.serviceId))
            {
                var id = offersalaryDTO.serviceId;

                if (await _context.providerServices.AnyAsync(s => s.Id == id))
                {
                    offersalaryDTO.serviceId = id;
                    offersalaryDTO.requestserviceId = null;
                }
                else if (await _context.requestServices.AnyAsync(r => r.Id == id))
                {
                    offersalaryDTO.requestserviceId = id;
                    offersalaryDTO.serviceId = null;
                }
                else
                {
                    offersalaryDTO.serviceId = null;
                    offersalaryDTO.requestserviceId = null;
                }
            }

            var offer = new OfferSalary
            {
                userId=userId,
                Salary = offersalaryDTO.Salary,
                Deliverytime = offersalaryDTO.Deliverytime,
                Notes = offersalaryDTO.Notes,
                serviceId = offersalaryDTO.serviceId,
                requestserviceId = offersalaryDTO.requestserviceId
            };

            
            await _context.offerSalaries.AddAsync(offer);
            await _context.SaveChangesAsync();



            var user =await _context.users.FirstOrDefaultAsync(u => u.Id == userId);
            if (offer.serviceId != null)
            {
                var providerService = await _context.providerServices
                    .Include(p => p.serviceProvider)
                    .FirstOrDefaultAsync(p => p.Id == offer.serviceId && p.serviceProvider.User.FcmToken != null);

                string title = "عرض سعر جديد";
                string body = $"تم تقديم عرض سعر على خدمتك من المستخدم {user.FirstName+" "+user.LastName}";

                var provviderr = await _context.users.FirstOrDefaultAsync(p => p.Id == providerService.serviceProvider.UserId);
                if (providerService?.serviceProvider != null)
                {
                    await _firebase.SendNotificationAsync(
                        provviderr.FcmToken,
                        title,
                        body
                    );

                    _context.notifications.Add(new Notifications
                    {
                        UserId = providerService.serviceProvider.UserId,
                        Title = title,
                        Body = body,
                        userImg=providerService.serviceProvider.Img,
                        CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                    }); 
                }
                await _context.SaveChangesAsync();
            }
            else if (offer.requestserviceId != null)
            {
                var requestService = await _context.requestServices
                    .Include(r => r.UserProfile)
                    .FirstOrDefaultAsync(r => r.Id == offer.requestserviceId && r.UserProfile.User.FcmToken != null);

                string title = "عرض سعر جديد";
                string body = $"تم تقديم عرض سعر على طلبك من موفر الخدمة {user.FirstName+" "+user.LastName}";

                var userr = await _context.users.FirstOrDefaultAsync(u => u.Id == requestService.UserProfile.UserId);
                if (requestService?.UserProfile != null)
                {
                    await _firebase.SendNotificationAsync(
                        userr.FcmToken,
                        title,
                        body
                    );

                    _context.notifications.Add(new Notifications
                    {
                        UserId = requestService.UserProfile.UserId,
                        Title = title,
                        Body = body,
                        userImg = requestService.UserProfile.Img,
                        CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                    });
                }
                await _context.SaveChangesAsync();
            }

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
                    userId = o.userId,
                    userName = o.User != null ? o.User.FirstName+" "+o.User.LastName : null,
                    userImg = o.serviceId != null
                        ? o.ProviderServices.providerImg
                        : o.RequestService.userImg != null
                            ? o.RequestService.userImg
                            : null

                    ,Salary = o.Salary,
                    Deliverytime = o.Deliverytime,
                    Notes = o.Notes,
                    serviceId = o.ProviderServices !=null ? o.ProviderServices.Id: o.requestserviceId,
                    ServiceName = o.ProviderServices != null ? o.ProviderServices.Name :
                        o.RequestService != null ? o.RequestService.Name : null
                })
                .ToListAsync();
        }

        public async Task<List<offersalaryDTO>> GetAllOffersByServiceId(string serviceId)
        {
            return await _context.offerSalaries
                .Select(o => new offersalaryDTO
                {
                    ID= o.Id,
                    userId = o.userId,
                    userName = o.User != null ? o.User.FirstName+" "+o.User.LastName : null,
                    userImg = o.serviceId != null
                        ? o.ProviderServices.providerImg
                        : o.RequestService.userImg != null
                            ? o.RequestService.userImg
                            : null,
                    Salary = o.Salary,
                    Deliverytime = o.Deliverytime,
                    Notes = o.Notes,
                    serviceId = o.ProviderServices != null ? o.ProviderServices.Id : o.requestserviceId,
                    ServiceName = o.ProviderServices != null ? o.ProviderServices.Name :
                        o.RequestService != null ? o.RequestService.Name : null
                })
                .Where(x => x.serviceId == serviceId)
                .ToListAsync();
        }

        public async Task<offersalaryDTO> GetOfferByIdAsync(string id)
        {
            return await _context.offerSalaries
                .Select(o => new offersalaryDTO
                {
                    ID = o.Id,
                    userId = o.userId,
                    userName = o.User != null ? o.User.FirstName+" "+o.User.LastName : null,
                    userImg = o.serviceId != null
                        ? o.ProviderServices.providerImg
                        : o.RequestService.userImg != null
                            ? o.RequestService.userImg
                            : null,
                    Salary = o.Salary,
                    Deliverytime = o.Deliverytime,
                    Notes = o.Notes,
                    serviceId = o.ProviderServices != null ? o.ProviderServices.Id : o.requestserviceId,
                    ServiceName = o.ProviderServices != null ? o.ProviderServices.Name :
                        o.RequestService != null ? o.RequestService.Name : null
                })
                .FirstOrDefaultAsync(x => x.ID == id);
        }
        public async Task<offersalaryDTO> GetOfferByserviceIdAsync(string serviceId)
        {
            return await _context.offerSalaries
                .Select(o => new offersalaryDTO
                {
                    ID = o.Id,
                    userId = o.userId,
                    userName = o.User != null ? o.User.FirstName+" "+o.User.LastName : null,
                    userImg = o.serviceId != null
                        ? o.ProviderServices.providerImg
                        : o.RequestService.userImg != null
                            ? o.RequestService.userImg
                            : null,
                    Salary = o.Salary,
                    Deliverytime = o.Deliverytime,
                    Notes = o.Notes,
                    serviceId = o.ProviderServices != null ? o.ProviderServices.Id : o.requestserviceId,
                    ServiceName = o.ProviderServices != null ? o.ProviderServices.Name :
                        o.RequestService != null ? o.RequestService.Name : null
                })
                .FirstOrDefaultAsync(x => x.serviceId == serviceId);
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
