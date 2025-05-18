using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
using Skilly.Persistence.Abstract;
using System.Globalization;
using System.Security.Claims;

namespace Skilly.API.Controllers.Areas.Provider
{
    [Route("api/Provider/[controller]")]
    [ApiController]
    public class ProviderServicesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProviderServicesController(IUnitOfWork unitOfWork)
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

        [HttpGet("getAllServices")]
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
                var services = await _unitOfWork.providerServiceRepository.GetSortedProviderServicesAsync(sortBy,userLat, userLon);
                if (services == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "No Services found." });
                }
                return StatusCode(StatusCodes.Status200OK, new { services });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("GetAllServicesBy/{categoryId}")]
        public async Task<IActionResult> GetservicesbycategoryId(string categoryId)
        {
            var service = await _unitOfWork.providerServiceRepository.GetAllservicesbyCategoryId(categoryId);
            if (service == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = "No services found for this category." });
            }
            return StatusCode(StatusCodes.Status200OK, new { service });
        }

        [HttpGet("GetAllServicesByproviderId")]
        public async Task<IActionResult> GetservicesbyuserId()
        {
            string userId = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authorized.");
            }
            var service = await _unitOfWork.providerServiceRepository.GetAllServicesByproviderId(userId);
            if (service == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = "No services found for this provider." });
            }
            return StatusCode(StatusCodes.Status200OK, new { service });
        }


        [HttpGet("GetAllServicesByAnother/{providerId}")]
        public async Task<IActionResult> GetservicesbyuserId(string providerId)
        {
            
            var service = await _unitOfWork.providerServiceRepository.GetAllServicesByproviderId(providerId);
            if (service == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = "No services found for this provider." });
            }
            return StatusCode(StatusCodes.Status200OK, new { service });
        }

        [HttpGet("GetServiceBy/{serviceId}")]
        public async Task<IActionResult> GetServiceById([FromRoute] string serviceId)
        {
            try
            {
                var service = await _unitOfWork.providerServiceRepository.GetProviderServiceByIdAsync(serviceId);
                return StatusCode(StatusCodes.Status200OK, new { service });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = ex.Message });
            }
        }

        [HttpPost("AddService")]
        public async Task<IActionResult> AddService([FromForm] ProviderservicesDTO providerservicesDTO)
        {
            if (providerservicesDTO == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid service data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User not authorized.");
                }
                await _unitOfWork.providerServiceRepository.AddProviderService(providerservicesDTO, userId);

                return StatusCode(StatusCodes.Status200OK, new { message = "Service added successfully.", data = providerservicesDTO });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = ex.Message });
            }
        }

        [HttpPut("EditServiceBy/{serviceId}")]
        public async Task<IActionResult> EditService([FromForm] ProviderservicesDTO providerservicesDTO, [FromRoute] string serviceId)
        {
            if (providerservicesDTO == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid service data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User not authorized.");
                }
                await _unitOfWork.providerServiceRepository.EditProviderService(providerservicesDTO, userId, serviceId);

                return StatusCode(StatusCodes.Status200OK, new { message = "Service updated successfully.", data = providerservicesDTO });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteServiceBy/{serviceId}")]
        public async Task<IActionResult> DeleteService([FromRoute] string serviceId)
        {
            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.providerServiceRepository.DeleteProviderServiceAsync(serviceId, userId);

                return StatusCode(StatusCodes.Status200OK, new { message = "Service deleted successfully." });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = ex.Message });
            }
        }

        [HttpGet("get-all-DiscountServices")]
        public async Task<IActionResult> GetAllDiscountedServices()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null || user.Latitude == null || user.Longitude == null)
                    return BadRequest("User location not available.");

                var userLat = user.Latitude.Value;
                var userLon = user.Longitude.Value;
                var services = await _unitOfWork.providerServiceRepository.GetAllProviderServiceDiscounted(userLat, userLon);
                if (services == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "No Services found." });
                }
                return StatusCode(StatusCodes.Status200OK, new { services });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Internal server error: {ex.Message}" });
            }
        }


        [HttpPost("apply-Discount")]
        public async Task<IActionResult> ApplyDiscount([FromQuery] string serviceId)
        {
            if (serviceId == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { message = "Invalid service data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User not authorized.");
                }

                await _unitOfWork.providerServiceRepository.UseServiceDiscount(serviceId, userId);

                return StatusCode(StatusCodes.Status200OK, new { message = "Service Discounted successfully.", serviceID = serviceId });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { message = ex.Message });
            }
        }

    }
}
