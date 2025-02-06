using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml.Permissions;
using Vonage.Users;

namespace Skilly.Persistence.Implementation
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public CategoryRepository(ApplicationDbContext context,IMapper mapper,IImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }
        public async Task AddCategoryAsync(CategoryDTO cat)
        {
            var category= _mapper.Map<Category>(cat);
            var path = @"Images/Category/";
            if (cat.Img != null)
            {
               category.Img = await _imageService.SaveFileAsync(cat.Img, path);
            }
            await _context.categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(string id)
        {
            var cat = await _context.categories.FindAsync(id);
            _context.categories.Remove(cat);
        }

        public async Task<IEnumerable<Category>> GetAllCategoryAsync()
        {
            var cat= await _context.categories
                .ToListAsync();
            if (cat == null || !cat.Any())
            {
                return new List<Category>();
            }
            return cat;

        }

        public async Task<Category> GetCategoryByIdAsync(string id)
        {
            var cat= await _context.categories
                .FirstOrDefaultAsync(c=>c.Id == id);
            if(cat == null)
            {
                throw new Exception("Category Not Found");
            }
            return cat;
        }

        public async Task UpdateCategoryAsync(CategoryDTO cat,string id)
        {
            var category = await _context.categories.FirstOrDefaultAsync(c=>c.Id == id);
            if (category == null)
            {
                throw new Exception("Category Not Found");
            }
            _mapper.Map(category,cat);
            var path = @"Images/Category/";
            if (cat.Img != null)
            {
                if (!string.IsNullOrEmpty(category.Img))
                {
                    string imagePath = Path.Combine(path, category.Img);
                    await _imageService.DeleteFileAsync(category.Img);
                }

                category.Img = await _imageService.SaveFileAsync(cat.Img, path);
            }
            _context.categories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}
