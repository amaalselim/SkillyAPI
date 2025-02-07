using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.Implementation;
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
                //string userId = GetUserIdFromClaims();
                var galleries = await _unitOfWork.servicegalleryRepository.GetAllServiceGallery();
                if (galleries == null)
                {
                    return NotFound(new { message = "No service galleries found." });
                }

                return Ok(new { galleries });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("GetAllServicegalleryByproviderId")]
        public async Task<ActionResult<Servicesgallery>> GetservicesbyuserId()
        {
            string userId = GetUserIdFromClaims();
            var user = await _unitOfWork.servicegalleryRepository.GetAllgalleryByproviderId(userId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }


        [HttpGet("GetGalleryBy/{galleryId}")]
        public async Task<IActionResult> GetServiceGalleryById([FromRoute] string galleryId)
        {
            try
            {
                string userId = GetUserIdFromClaims();
                var gallery = await _unitOfWork.servicegalleryRepository.GetServiceGalleryByIdAsync(galleryId, userId);

                return Ok(new { gallery });
            }
            catch (ServiceGalleryNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpPost("AddGallery")]
        public async Task<IActionResult> AddServiceGallery([FromForm] servicegalleryDTO servicegalleryDTO)
        {
            if (servicegalleryDTO == null)
            {
                return BadRequest(new { message = "Invalid service gallery data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.servicegalleryRepository.AddServiceGallery(servicegalleryDTO, userId);

                return Ok(new { message = "Service gallery added successfully.", data = servicegalleryDTO });
            }
            catch (ServiceProviderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpPut("EditgalleryBy/{galleryId}")]
        public async Task<IActionResult> EditServiceGallery([FromForm] servicegalleryDTO servicegalleryDTO, [FromRoute] string galleryId)
        {
            if (servicegalleryDTO == null)
            {
                return BadRequest(new { message = "Invalid service gallery data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.servicegalleryRepository.EditServiceGallery(servicegalleryDTO, userId, galleryId);

                return Ok(new { message = "Service gallery updated successfully.", data = servicegalleryDTO });
            }
            catch (ServiceGalleryNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpDelete("DeleteGalleryBy/{galleryId}")]
        public async Task<IActionResult> DeleteServiceGallery([FromRoute] string galleryId)
        {
            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.servicegalleryRepository.DeleteServiceGalleryAsync(galleryId, userId);

                return Ok(new { message = "Service gallery deleted successfully." });
            }
            catch (ServiceGalleryNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
