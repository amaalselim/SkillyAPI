using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

namespace Skilly.API.Controllers.Areas.Provider
{
    [Route("api/Provider/[controller]")]
    [ApiController]
    public class ServicegalleryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServicegalleryController(IUnitOfWork unitOfWork)
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

        [HttpGet("getAllGallery")]
        public async Task<IActionResult> GetAllServiceGallery()
        {
            try
            {
                var galleries = await _unitOfWork.servicegalleryRepository.GetAllServiceGallery();
                if (galleries == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "No service galleries found." });
                }

                return StatusCode(StatusCodes.Status200OK, new { galleries });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("GetAllServicegalleryByproviderId")]
        public async Task<IActionResult> GetservicesbyuserId()
        {
            string userId = GetUserIdFromClaims();
            var servicesgallery = await _unitOfWork.servicegalleryRepository.GetAllgalleryByProviderId(userId);
            if (servicesgallery == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = "No galleries found for this provider." });
            }
            return StatusCode(StatusCodes.Status200OK, new { servicesgallery });
        }
        [HttpGet("GetAllServicegalleryBy/{providerId}")]
        public async Task<IActionResult> GetservicesbyproviderId(string providerId)
        {
            //string userId = GetUserIdFromClaims();
            var servicesgallery = await _unitOfWork.servicegalleryRepository.GetAllgalleryByPProviderId(providerId);
            if (servicesgallery == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = "No galleries found for this provider." });
            }
            return StatusCode(StatusCodes.Status200OK, new { servicesgallery });
        }

        [HttpGet("GetGalleryBy/{galleryId}")]
        public async Task<IActionResult> GetServiceGalleryById([FromRoute] string galleryId)
        {
            try
            {
                string userId = GetUserIdFromClaims();
                var gallery = await _unitOfWork.servicegalleryRepository.GetServiceGalleryByIdAsync(galleryId, userId);

                return StatusCode(StatusCodes.Status200OK, new { gallery });
            }
            catch (ServiceGalleryNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = ex.Message });
            }
        }

        [HttpPost("AddGallery")]
        public async Task<IActionResult> AddServiceGallery([FromForm] servicegalleryDTO servicegalleryDTO)
        {
            if (servicegalleryDTO == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid service gallery data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.servicegalleryRepository.AddServiceGallery(servicegalleryDTO, userId);

                return StatusCode(StatusCodes.Status200OK, new { message = "Service gallery added successfully.", data = servicegalleryDTO });
            }
            catch (ServiceProviderNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = ex.Message });
            }
        }

        [HttpPut("EditgalleryBy/{galleryId}")]
        public async Task<IActionResult> EditServiceGallery([FromForm] servicegalleryDTO servicegalleryDTO, [FromRoute] string galleryId)
        {
            if (servicegalleryDTO == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid service gallery data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.servicegalleryRepository.EditServiceGallery(servicegalleryDTO, userId, galleryId);

                return StatusCode(StatusCodes.Status200OK, new { message = "Service gallery updated successfully.", data = servicegalleryDTO });
            }
            catch (ServiceGalleryNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteGalleryBy/{galleryId}")]
        public async Task<IActionResult> DeleteServiceGallery([FromRoute] string galleryId)
        {
            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.servicegalleryRepository.DeleteServiceGalleryAsync(galleryId, userId);

                return StatusCode(StatusCodes.Status200OK, new { message = "Service gallery deleted successfully." });
            }
            catch (ServiceGalleryNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = ex.Message });
            }
        }
    }
}
