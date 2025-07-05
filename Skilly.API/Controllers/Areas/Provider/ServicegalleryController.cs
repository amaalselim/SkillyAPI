using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs.ServiceProvider;
using Skilly.Application.Exceptions;
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
                throw new UnauthorizedAccessException("User not authorized.");
            return userId;
        }

        [HttpGet("getAllGallery")]
        public async Task<IActionResult> GetAllServiceGallery()
        {
            try
            {
                var galleries = await _unitOfWork.servicegalleryRepository.GetAllServiceGallery();
                if (galleries == null || !galleries.Any())
                    return NotFound(new { message = "No service galleries found." });

                return Ok(new { galleries });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetAllServicegalleryByproviderId")]
        public async Task<IActionResult> GetGalleriesByLoggedInProvider()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var galleries = await _unitOfWork.servicegalleryRepository.GetAllgalleryByProviderId(userId);
                if (galleries == null || !galleries.Any())
                    return NotFound(new { message = "No galleries found for this provider." });

                return Ok(new { galleries });
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

        [HttpGet("GetAllServicegalleryBy/{providerId}")]
        public async Task<IActionResult> GetGalleriesByProviderId(string providerId)
        {
            try
            {
                var galleries = await _unitOfWork.servicegalleryRepository.GetAllgalleryByPProviderId(providerId);
                if (galleries == null || !galleries.Any())
                    return NotFound(new { message = "No galleries found for this provider." });

                return Ok(new { galleries });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetGalleryBy/{galleryId}")]
        public async Task<IActionResult> GetServiceGalleryById([FromRoute] string galleryId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var gallery = await _unitOfWork.servicegalleryRepository.GetServiceGalleryByIdAsync(galleryId, userId);

                return Ok(new { gallery });
            }
            catch (ServiceGalleryNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("AddGallery")]
        public async Task<IActionResult> AddServiceGallery([FromForm] servicegalleryDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid service gallery data." });

            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork.servicegalleryRepository.AddServiceGallery(dto, userId);

                return Ok(new { message = "Service gallery added successfully.", data = dto });
            }
            catch (ServiceProviderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("EditgalleryBy/{galleryId}")]
        public async Task<IActionResult> EditServiceGallery([FromForm] editgalleryDTO dto, [FromRoute] string galleryId)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid service gallery data." });

            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork.servicegalleryRepository.EditServiceGallery(dto, userId, galleryId);

                return Ok(new { message = "Service gallery updated successfully.", data = dto });
            }
            catch (ServiceGalleryNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteGalleryBy/{galleryId}")]
        public async Task<IActionResult> DeleteServiceGallery([FromRoute] string galleryId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork.servicegalleryRepository.DeleteServiceGalleryAsync(galleryId, userId);

                return Ok(new { message = "Service gallery deleted successfully." });
            }
            catch (ServiceGalleryNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
