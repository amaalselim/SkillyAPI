using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using Skilly.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoryAsync();
        Task<Category> GetCategoryByIdAsync(string id);
        Task AddCategoryAsync(CategoryDTO category);
        Task UpdateCategoryAsync(CategoryDTO cat, string id);
        Task DeleteCategoryAsync(string id);

    }
}
