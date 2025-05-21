using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Exchange.WebServices.Data;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

namespace Skilly.API.Controllers.Areas.userProfile
{
    [Route("api/UserProfile/[controller]")]
    [ApiController]
    public class requestServicesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public requestServicesController(IUnitOfWork unitOfWork)
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

        [HttpGet("GetAllRequests")]
        public async Task<IActionResult> GetAllServices([FromQuery] string sortBy = "nearest")
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null || user.Latitude == null || user.Longitude == null)
                    return BadRequest("User location not available.");

                var userLat = user.Latitude.Value;
                var userLon = user.Longitude.Value;
                var services = await _unitOfWork._requestserviceRepository.GetSortedUserAsync(sortBy, userLat, userLon);
                if (services == null || !services.Any())
                {
                    return NotFound(new { message = "No services found." });
                }
                return StatusCode(StatusCodes.Status200OK, new { services });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("GetAllRequestsByCategoryId")]
        public async Task<IActionResult> GetServicesBycategoryId()
        {
            try
            {
                string userId = GetUserIdFromClaims();
                var services = await _unitOfWork._requestserviceRepository.GetAllRequestsByCategoryId(userId);
                if (services == null || !services.Any())
                {
                    return NotFound(new { message = "No services found for this user." });
                }
                return Ok(new { services });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("GetAllRequestsByuserId")]
        public async Task<IActionResult> GetServicesByUserId()
        {
            try
            {
                string userId = GetUserIdFromClaims();
                var services = await _unitOfWork._requestserviceRepository.GetAllRequestsByUserId(userId);
                if (services == null || !services.Any())
                {
                    return NotFound(new { message = "No services found for this user." });
                }
                return Ok(new { services });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("GetRequestsBy/{serviceId}")]
        public async Task<IActionResult> GetServiceById([FromRoute] string serviceId)
        {
            try
            {
                var service = await _unitOfWork._requestserviceRepository.GetRequestById(serviceId);
                if (service == null)
                {
                    return NotFound(new { message = "Service not found." });
                }
                return Ok(new { service });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("AddrequestService")]
        public async Task<IActionResult> AddService([FromForm] requestServiceDTO requestServiceDTO)
        {
            if (requestServiceDTO == null)
            {
                return BadRequest(new { message = "Invalid service data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork._requestserviceRepository.AddRequestService(requestServiceDTO, userId);

                return StatusCode(201, new { message = "Service added successfully.", data = requestServiceDTO });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("EditrequestServiceBy/{serviceId}")]
        public async Task<IActionResult> EditService([FromForm] requestServiceDTO requestServiceDTO, [FromRoute] string serviceId)
        {
            if (requestServiceDTO == null)
            {
                return BadRequest(new { message = "Invalid service data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                var service = await _unitOfWork._requestserviceRepository.GetRequestById(serviceId);
                if (service == null)
                {
                    return NotFound(new { message = "Service not found." });
                }

                await _unitOfWork._requestserviceRepository.EditRequestService(requestServiceDTO, userId, serviceId);

                return Ok(new { message = "Service updated successfully.", data = requestServiceDTO });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpDelete("DeleterequestServiceBy/{serviceId}")]
        public async Task<IActionResult> DeleteService([FromRoute] string serviceId)
        {
            try
            {
                string userId = GetUserIdFromClaims();
                var service = await _unitOfWork._requestserviceRepository.GetRequestById(serviceId);
                if (service == null)
                {
                    return NotFound(new { message = "Service not found." });
                }

                await _unitOfWork._requestserviceRepository.DeleteRequestServiceAsync(serviceId, userId);

                return Ok(new { message = "Service deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpPost("AcceptService/{requestId}")]
        public async Task<IActionResult> AcceptService(string requestId)
        {
            try
            {
                string userId = GetUserIdFromClaims();
                var service = await _unitOfWork._requestserviceRepository.GetRequestById(requestId);
                if (service == null)
                {
                    return NotFound(new { message = "Service not found." });
                }
                await _unitOfWork._requestserviceRepository.AcceptService(requestId, userId);
                return Ok(new { message = "Service accepted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
