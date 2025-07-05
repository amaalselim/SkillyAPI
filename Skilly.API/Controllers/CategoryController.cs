using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _unitOfWork._categoryRepository.GetAllCategoryAsync();

            if (categories == null || !categories.Any())
                return NotFound(new { message = "No categories found." });

            return Ok(new { categories });
        }

        [HttpGet("GetCategoryBy/{categoryId}")]
        public async Task<IActionResult> GetCategoryById(string categoryId)
        {
            var category = await _unitOfWork._categoryRepository.GetCategoryByIdAsync(categoryId);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(new { category });
        }

        [HttpPost("AddCategory")]
        [Authorize]
        public async Task<IActionResult> AddCategory([FromForm] CategoryDTO categoryDTO)
        {
            if (categoryDTO == null)
                return BadRequest(new { message = "Invalid category data." });

            await _unitOfWork._categoryRepository.AddCategoryAsync(categoryDTO);

            return CreatedAtAction(nameof(GetCategoryById), new
            {
                message = "Category added successfully.",
                data = categoryDTO
            });
        }

        [HttpPut("EditCategoryBy/{categoryId}")]
        [Authorize]
        public async Task<IActionResult> EditCategory([FromForm] CategoryDTO categoryDTO, string categoryId)
        {
            if (categoryDTO == null)
                return BadRequest(new { message = "Invalid category data." });

            var existing = await _unitOfWork._categoryRepository.GetCategoryByIdAsync(categoryId);

            if (existing == null)
                return NotFound(new { message = "Category not found." });

            await _unitOfWork._categoryRepository.UpdateCategoryAsync(categoryDTO, categoryId);

            return Ok(new
            {
                message = "Category updated successfully.",
                data = categoryDTO
            });
        }

        [HttpDelete("DeleteCategoryBy/{categoryId}")]
        [Authorize]
        public async Task<IActionResult> DeleteCategory(string categoryId)
        {
            var existing = await _unitOfWork._categoryRepository.GetCategoryByIdAsync(categoryId);

            if (existing == null)
                return NotFound(new { message = "Category not found." });

            await _unitOfWork._categoryRepository.DeleteCategoryAsync(categoryId);

            return Ok(new { message = "Category deleted successfully." });
        }
    }
}
