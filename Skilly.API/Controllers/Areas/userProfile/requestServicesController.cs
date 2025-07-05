using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs.User;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

namespace Skilly.API.Controllers.Areas.userProfile
{
    [Route("api/UserProfile/[controller]")]
    [ApiController]
    public class RequestServicesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RequestServicesController(IUnitOfWork unitOfWork)
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

        [HttpGet("GetAllRequests")]
        public async Task<IActionResult> GetAllServices([FromQuery] string sortBy = "nearest")
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user?.Latitude == null || user?.Longitude == null)
                    return BadRequest(new { message = "User location not available." });

                var services = await _unitOfWork._requestserviceRepository.GetSortedUserAsync(sortBy, userId, user.Latitude.Value, user.Longitude.Value);

                return Ok(new { services });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetAllUserRequests")]
        public async Task<IActionResult> GetAllUserServices()
        {
            try
            {
                var services = await _unitOfWork._requestserviceRepository.GetAllRequests();
                return Ok(new { services });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetAllRequestsByCategoryId")]
        public async Task<IActionResult> GetServicesByCategoryId([FromQuery] string sortBy = "nearest")
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user?.Latitude == null || user?.Longitude == null)
                    return BadRequest(new { message = "User location not available." });

                var services = await _unitOfWork._requestserviceRepository.GetAllRequestsByCategoryId(userId, sortBy, user.Latitude.Value, user.Longitude.Value);

                return Ok(new { services });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetAllRequestsByUserId")]
        public async Task<IActionResult> GetServicesByUserId()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var services = await _unitOfWork._requestserviceRepository.GetAllRequestsByUserId(userId);
                return Ok(new { services });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetRequestBy/{serviceId}")]
        public async Task<IActionResult> GetServiceById(string serviceId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var service = await _unitOfWork._requestserviceRepository.GetRequestById(serviceId, userId);

                if (service == null)
                    return NotFound(new { message = "Service not found." });

                return Ok(new { service });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("AddRequestService")]
        [Authorize]
        public async Task<IActionResult> AddService([FromForm] requestServiceDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid service data." });

            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork._requestserviceRepository.AddRequestService(dto, userId);

                return StatusCode(201, new { message = "Service added successfully.", data = dto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("EditRequestServiceBy/{serviceId}")]
        [Authorize]
        public async Task<IActionResult> EditService([FromForm] EditRequestServiceDTO dto, string serviceId)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid service data." });

            try
            {
                var userId = GetUserIdFromClaims();
                var service = await _unitOfWork._requestserviceRepository.GetRequestById(serviceId, userId);

                if (service == null)
                    return NotFound(new { message = "Service not found." });

                await _unitOfWork._requestserviceRepository.EditRequestService(dto, userId, serviceId);

                return Ok(new { message = "Service updated successfully.", data = dto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteRequestServiceBy/{serviceId}")]
        [Authorize]
        public async Task<IActionResult> DeleteService(string serviceId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var service = await _unitOfWork._requestserviceRepository.GetRequestById(serviceId, userId);

                if (service == null)
                    return NotFound(new { message = "Service not found." });

                await _unitOfWork._requestserviceRepository.DeleteRequestServiceAsync(serviceId, userId);

                return Ok(new { message = "Service deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("AcceptService/{requestId}")]
        public async Task<IActionResult> AcceptService(string requestId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork._requestserviceRepository.AcceptService(requestId, userId);
                return Ok(new { message = "Service accepted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("TrackRequestService/{serviceId}")]
        public async Task<IActionResult> TrackRequestService(string serviceId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var result = await _unitOfWork._requestserviceRepository.TrackRequestServiceAsync(serviceId, userId);

                if (result == null)
                    return NotFound(new { message = "Request service not found." });

                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
