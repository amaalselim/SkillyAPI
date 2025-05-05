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
                return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid review data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.reviewRepository.AddReviewAsync(userId, reviewDTO);

                return StatusCode(StatusCodes.Status200OK, new { message = "Review added successfully.", data = new { reviewDTO } });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error: {ex.Message}" });
            }
        }


        [HttpGet("GetReviewsByproviderId")]
        public async Task<IActionResult> GetAllReviewsByProvider()
        {
            
            try
            {
                var providerId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(providerId))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid provider ID." });
                }

                var reviews = await _unitOfWork.reviewRepository.GetAllReviewsByProviderIdAsync(providerId);
                return StatusCode(StatusCodes.Status200OK, new { reviews });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("average-ratingByproviderId")]
        public async Task<IActionResult> GetAverageRating()
        {
            var providerId = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(providerId))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid provider ID." });
            }
            var averageRating = await _unitOfWork.reviewRepository.GetAverageRatingByProviderIdAsync(providerId);
            return Ok(new { averageRating });
        }
    }
}
