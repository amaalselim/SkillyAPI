using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

namespace Skilly.API.Controllers.Areas.Provider
{
    [Route("api/Provider/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewController(IUnitOfWork unitOfWork)
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

        [HttpPost("AddReview")]
        public async Task<IActionResult> AddReview([FromBody] ReviewDTO reviewDTO)
        {
            if (reviewDTO == null)
            {
                return BadRequest(new { message = "Invalid review data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.reviewRepository.AddReviewAsync(userId, reviewDTO);

                return Ok(new { message = "Review added successfully.", data = reviewDTO });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("GetReviewsBy/{providerId}")]
        public async Task<IActionResult> GetAllReviewsByProvider([FromRoute] string providerId)
        {
            if (string.IsNullOrEmpty(providerId))
            {
                return BadRequest(new { message = "Provider ID is required." });
            }

            try
            {
                var reviews = await _unitOfWork.reviewRepository.GetAllReviewsByProviderIdAsync(providerId);
                return Ok(new { reviews });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
