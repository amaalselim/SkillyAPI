using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
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
    public class BannerService : IBannerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public BannerService(ApplicationDbContext context,IImageService imageService,IMapper mapper)
        {
            _context = context;
            _imageService = imageService;
            _mapper = mapper;
        }
        public async Task<IEnumerable<Banner>> GetAllBannerAsync()
        {
            var banner = await _context.banners
                .ToListAsync();
            if (banner == null || !banner.Any())
            {
                return new List<Banner>();
            }
            return banner;
        }

        public async Task<Banner> UploadBannerAsync(BannerCreateDTO bannerCreateDTO)
        {
            var banner=_mapper.Map<Banner>(bannerCreateDTO);    
            var path = @"Images/Banner/";
            if(bannerCreateDTO.Image!= null)
            {
                banner.ImagePath = await _imageService.SaveFileAsync(bannerCreateDTO.Image, path);
            }
            await _context.banners.AddAsync(banner);
            await _context.SaveChangesAsync();
            return banner;
        }

        public async Task DeleteBannerAsync(string id)
        {
            var banner = await _context.banners.FindAsync(id);

            _context.banners.Remove(banner);
            await _context.SaveChangesAsync();
        }
    }
}
