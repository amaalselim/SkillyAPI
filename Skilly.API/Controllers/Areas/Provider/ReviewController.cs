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
            {
                throw new UnauthorizedAccessException("User not authorized.");
            }
            return userId;
        }
     

        [HttpPost("AddServiceReview")]
        public async Task<IActionResult> AddserviceReview([FromBody] ReviewServiceDTO reviewDTO)
        {
            if (reviewDTO == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid review data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.reviewRepository.AddReviewserviceAsync(userId, reviewDTO);

                return StatusCode(StatusCodes.Status200OK, new { message = "Review added successfully.", data = new { reviewDTO } });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("GetAllReviews")]
        public async Task<IActionResult> GetAllReviews()
        {
            try
            {
                var reviews = await _unitOfWork.reviewRepository.GetAllReviews();
                return StatusCode(StatusCodes.Status200OK, new { reviews });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("GetReviewsBy/{serviceId}")]
        public async Task<IActionResult> GetAllReviewsByservice(string serviceId)
        {

            try
            {
                //var providerId = GetUserIdFromClaims();
                //if (string.IsNullOrEmpty(providerId))
                //{
                //    return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid provider ID." });
                //}

                var reviews = await _unitOfWork.reviewRepository.GetAllReviewsByserviceIdAsync(serviceId);
                return StatusCode(StatusCodes.Status200OK, new { reviews });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("GetAllReviewsByproviderId")]
        public async Task<IActionResult> GetAllReviewsByProvider()
        {
            try
            {
                var providerId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(providerId))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid provider ID." });
                }

                var reviews = await _unitOfWork.reviewRepository.GetAllReviewsByproviderIdAsync(providerId);
                return StatusCode(StatusCodes.Status200OK, new { reviews });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error: {ex.Message}" });
            }
        }

       



        [HttpGet("GetAllReviewsBy/{providerId}")]
        public async Task<IActionResult> GetAllReviewsByProvider(string providerId)
        {
            
            try
            {
                var reviews = await _unitOfWork.reviewRepository.GetAllReviewsByproviderIdAsync(providerId);
                return StatusCode(StatusCodes.Status200OK, new { reviews });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        
    }
}
