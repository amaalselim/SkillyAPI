using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet("getAllRequests")]
        public async Task<IActionResult> GetAllServices()
        {
            try
            {
                //string userId = GetUserIdFromClaims();
                var Services = await _unitOfWork._requestserviceRepository.GetAllRequests();
                if (Services == null)
                {
                    return NotFound(new { message = "No Services found." });
                }

                return Ok(new { Services });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("GetAllRequestsByuserId")]
        public async Task<ActionResult<ProviderServices>> GetservicesbyuserId()
        {
            string userId = GetUserIdFromClaims();
            var services = await _unitOfWork._requestserviceRepository.GetAllRequestsByUserId(userId);
            if (services == null)
            {
                return NotFound();
            }
            return Ok(new { services });
        }
        [HttpGet("GeyrequestsBy/{serviceId}")]
        public async Task<IActionResult> GetServiceById([FromRoute] string serviceId)
        {
            try
            {
                string userId = GetUserIdFromClaims();
                var service = await _unitOfWork._requestserviceRepository.GetRequestById(serviceId, userId);

                return Ok(new { service });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
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

                return Ok(new { message = "Service added successfully.", data = requestServiceDTO});
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpPut("EditsequestServiceBy/{serviceId}")]
        public async Task<IActionResult> EditService([FromForm] requestServiceDTO requestServiceDTO, [FromRoute] string serviceId)
        {
            if (requestServiceDTO== null)
            {
                return BadRequest(new { message = "Invalid service data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork._requestserviceRepository.EditRequestService(requestServiceDTO, userId, serviceId);

                return Ok(new { message = " service updated successfully.", data = requestServiceDTO });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpDelete("DeleterequestServiceBy/{serviceId}")]
        public async Task<IActionResult> DeleteService([FromRoute] string serviceId)
        {
            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork._requestserviceRepository.DeleteRequestServiceAsync(serviceId, userId);

                return Ok(new { message = "Service deleted successfully." });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

    }
}
