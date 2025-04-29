using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceProvider = Skilly.Core.Entities.ServiceProvider;

namespace Skilly.Persistence.Implementation
{
    public class ServiceProviderRepository : IServiceProviderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public ServiceProviderRepository(ApplicationDbContext context, IMapper mapper, IImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }
        public async Task AddServiceProviderAsync(ServiceProviderDTO ServiceProviderDTO, string userId)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found.");
            }
            var existingProfile = await _context.serviceProviders.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existingProfile != null)
            {
                throw new InvalidOperationException("User profile already exists.");
            }
            var ServiceProvider = _mapper.Map<ServiceProvider>(ServiceProviderDTO);
            ServiceProvider.UserId = userId;
            ServiceProvider.FirstName = user.FirstName;
            ServiceProvider.LastName = user.LastName;
            ServiceProvider.Email = user.Email;
            ServiceProvider.PhoneNumber = user.PhoneNumber;
            if (ServiceProviderDTO.Img != null)
            {
                var path = @"Images/ServiceProvider/";
                ServiceProvider.Img = await _imageService.SaveFileAsync(ServiceProviderDTO.Img, path);
            }
            await _context.serviceProviders.AddAsync(ServiceProvider);
            await _context.SaveChangesAsync();
        }
        public async Task EditServiceProviderAsync(ServiceProviderDTO ServiceProviderDTO, string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found.");
            }

            var ServiceProvider = await _context.serviceProviders.FirstOrDefaultAsync(up => up.UserId == userId);
            if (ServiceProvider == null)
            {
                throw new ServiceProviderNotFoundException("User Profile not found.");
            }

            _mapper.Map(ServiceProviderDTO, ServiceProvider);

            ServiceProvider.FirstName = user.FirstName;
            ServiceProvider.LastName = user.LastName;
            ServiceProvider.Email = user.Email;
            ServiceProvider.PhoneNumber = user.PhoneNumber;

            if (ServiceProviderDTO.Img != null)
            {
                var path = @"Images/ServiceProvider/";
                ServiceProvider.Img = await _imageService.SaveFileAsync(ServiceProviderDTO.Img, path);
            }
            if (ServiceProviderDTO.NationalNumberPDF != null)
            {
                var path = @"Images/ServiceProvider/File";
                ServiceProvider.NationalNumberPDF = await _imageService.SaveFileAsync(ServiceProviderDTO.NationalNumberPDF, path);
            }

            _context.serviceProviders.Update(ServiceProvider);
            await _context.SaveChangesAsync();
        }

        public async Task<ServiceProvider> GetByIdAsync(string id)
        {
            return await _context.serviceProviders
                .Include(p => p.providerServices)
                .ThenInclude(p => p.ServicesImages)
                .Include(p => p.servicesgalleries)
                .ThenInclude(p => p.galleryImages)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.UserId ==id);
        }

        public async Task<List<ServiceProvider>> GetAllServiceProviderAsync()
        {
            var users = await _context.serviceProviders
                //.Include(p=>p.providerServices)
                //.ThenInclude(p=>p.ServicesImages)
                //.Include(p=>p.servicesgalleries)
                //.ThenInclude(p=>p.Images)
                //.Include(p=>p.Reviews)
                .ToListAsync();

            if (users == null || !users.Any())
            {
                return new List<ServiceProvider>();
            }
            return users;
        }

        public async Task DeleteServiceProviderAsync(string Id)
        {

            var ServiceProvider = await _context.serviceProviders.FirstOrDefaultAsync(up => up.UserId == Id);
            if (ServiceProvider == null)
            {
                throw new ServiceProviderNotFoundException("User profile not found.");
            }
            _context.serviceProviders.Remove(ServiceProvider);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ServiceProvider>> GetAllserviceProvidersbyCategoryId(string categoryId)
        {
            return await _context.serviceProviders.Where(c => c.categoryId == categoryId)
                //.Include(p => p.providerServices)
                //.ThenInclude(p => p.ServicesImages)
                //.Include(p => p.servicesgalleries)
                //.ThenInclude(p => p.Images)
                //.Include(p => p.Reviews)
                .ToListAsync();
        }
    }
}
