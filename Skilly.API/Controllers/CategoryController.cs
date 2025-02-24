using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
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
        private string GetUserIdFromClaims()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authorized.");
            }
            return userId;
        }
        [HttpGet("getAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _unitOfWork._categoryRepository.GetAllCategoryAsync();

                if (categories == null || !categories.Any())
                {
                    return NotFound(new { message = "No Categories found." });
                }
               return Ok(new { categories });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }


        [HttpGet("GetCategoryBy/{categoryId}")]
        public async Task<IActionResult> GetCategoryById([FromRoute] string categoryId)
        {
            try
            {
                var category = await _unitOfWork._categoryRepository.GetCategoryByIdAsync(categoryId);

                return Ok(new { category });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpPost("AddCategory")]
        public async Task<IActionResult> AddCategory([FromForm] CategoryDTO categoryDTO)
        {
            if (categoryDTO == null)
            {
                return BadRequest(new { message = "Invalid Category data." });
            }

            try
            {
                await _unitOfWork._categoryRepository.AddCategoryAsync(categoryDTO);

                return Ok(new { message = "Category added successfully.", data = categoryDTO });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpPut("EditCategoryBy/{categoryId}")]
        public async Task<IActionResult> EditCategory([FromForm] CategoryDTO categoryDTO, [FromRoute] string categoryId)
        {
            if (categoryDTO == null)
            {
                return BadRequest(new { message = "Invalid Category data." });
            }

            try
            {
                await _unitOfWork._categoryRepository.UpdateCategoryAsync(categoryDTO, categoryId);

                return Ok(new { message = "Category updated successfully.", data = categoryDTO });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpDelete("DeleteCategoryBy/{categoryId}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] string categoryId)
        {
            try
            {
               await _unitOfWork._categoryRepository.DeleteCategoryAsync(categoryId);

                return Ok(new { message = "Category deleted successfully." });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
