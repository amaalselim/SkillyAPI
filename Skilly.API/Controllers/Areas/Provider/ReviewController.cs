using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs.Review;
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
                throw new UnauthorizedAccessException("User not authorized.");
            return userId;
        }

        [HttpPost("AddServiceReview")]
        public async Task<IActionResult> AddServiceReview([FromBody] ReviewServiceDTO reviewDTO)
        {
            if (reviewDTO == null)
                return BadRequest(new { message = "Invalid review data." });

            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork.reviewRepository.AddReviewserviceAsync(userId, reviewDTO);

                return Ok(new { message = "Review added successfully.", data = reviewDTO });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetAllReviews")]
        public async Task<IActionResult> GetAllReviews()
        {
            try
            {
                var reviews = await _unitOfWork.reviewRepository.GetAllReviews();
                if (reviews == null)
                    return NotFound(new { message = "No reviews found." });

                return Ok(new { reviews });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetReviewsBy/{serviceId}")]
        public async Task<IActionResult> GetAllReviewsByService(string serviceId)
        {
            try
            {
                var reviews = await _unitOfWork.reviewRepository.GetAllReviewsByserviceIdAsync(serviceId);
                if (reviews == null)
                    return NotFound(new { message = "No reviews found for this service." });

                return Ok(new { reviews });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetAllReviewsByproviderId")]
        public async Task<IActionResult> GetAllReviewsByLoggedInProvider()
        {
            try
            {
                var providerId = GetUserIdFromClaims();
                var reviews = await _unitOfWork.reviewRepository.GetAllReviewsByproviderIdAsync(providerId);

                if (reviews == null)
                    return NotFound(new { message = "No reviews found for this provider." });

                return Ok(new { reviews });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetAllReviewsBy/{providerId}")]
        public async Task<IActionResult> GetAllReviewsByProvider(string providerId)
        {
            try
            {
                var reviews = await _unitOfWork.reviewRepository.GetAllReviewsByproviderIdAsync(providerId);
                if (reviews == null)
                    return NotFound(new { message = "No reviews found for this provider." });

                return Ok(new { reviews });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
