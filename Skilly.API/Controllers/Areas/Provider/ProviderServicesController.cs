using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs.ServiceProvider;
using Skilly.Application.Exceptions;
using Skilly.Persistence.Abstract;
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
                throw new UnauthorizedAccessException("User not authorized.");
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
                    return BadRequest(new { message = "User location not available." });

                var services = await _unitOfWork.providerServiceRepository.GetSortedProviderServicesAsync(sortBy, user.Latitude.Value, user.Longitude.Value, userId);
                if (services == null || !services.Any())
                    return NotFound(new { message = "No services found." });

                return Ok(new { services });
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

        [HttpGet("getAllproviderServices")]
        public async Task<IActionResult> GetAllproviderServices()
        {
            try
            {
                var services = await _unitOfWork.providerServiceRepository.GetAllProviderService();
                if (services == null || !services.Any())
                    return NotFound(new { message = "No services found." });

                return Ok(new { services });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetAllServicesBy/{categoryId}")]
        public async Task<IActionResult> GetservicesbycategoryId(string categoryId, [FromQuery] string sortBy = "nearest")
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null || user.Latitude == null || user.Longitude == null)
                    return BadRequest(new { message = "User location not available." });

                var service = await _unitOfWork.providerServiceRepository.GetAllservicesbyCategoryId(userId, categoryId, sortBy, user.Latitude.Value, user.Longitude.Value);
                return Ok(new { service });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetAllServicesByproviderId")]
        public async Task<IActionResult> GetservicesbyuserId()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var service = await _unitOfWork.providerServiceRepository.GetAllServicesByproviderId(userId);
                return Ok(new { service });
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

        [HttpGet("GetAllServicesByAnother/{providerId}")]
        public async Task<IActionResult> GetservicesbyuserId(string providerId)
        {
            try
            {
                var service = await _unitOfWork.providerServiceRepository.GetAllServicesByproviderId(providerId);
                return Ok(new { service });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetServiceBy/{serviceId}")]
        public async Task<IActionResult> GetServiceById(string serviceId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var service = await _unitOfWork.providerServiceRepository.GetProviderServiceByIdAsync(userId, serviceId);
                return Ok(new { service });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("AddService")]
        public async Task<IActionResult> AddService([FromForm] ProviderservicesDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid service data." });

            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork.providerServiceRepository.AddProviderService(dto, userId);
                return Ok(new { message = "Service added successfully.", data = dto });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("EditServiceBy/{serviceId}")]
        public async Task<IActionResult> EditService([FromForm] EditProviderServiceDTO dto, string serviceId)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid service data." });

            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork.providerServiceRepository.EditProviderService(dto, userId, serviceId);
                return Ok(new { message = "Service updated successfully.", data = dto });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteServiceBy/{serviceId}")]
        public async Task<IActionResult> DeleteService(string serviceId)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork.providerServiceRepository.DeleteProviderServiceAsync(serviceId, userId);
                return Ok(new { message = "Service deleted successfully." });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
                    return BadRequest(new { message = "User location not available." });

                var services = await _unitOfWork.providerServiceRepository.GetAllProviderServiceDiscounted(user.Latitude.Value, user.Longitude.Value);
                if (services == null || !services.Any())
                    return NotFound(new { message = "No services found." });

                return Ok(new { services });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("apply-Discount/{serviceId}")]
        public async Task<IActionResult> ApplyDiscount(string serviceId)
        {
            if (string.IsNullOrEmpty(serviceId))
                return BadRequest(new { message = "Invalid service data." });

            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork.providerServiceRepository.UseServiceDiscount(serviceId, userId);
                return Ok(new { message = "Service discounted successfully.", serviceId });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("Get-Services-InProgress")]
        public async Task<IActionResult> GetservicesInProgress()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var service = await _unitOfWork.providerServiceRepository.GetAllServicesInProgress(userId);
                if (service == null)
                    return NotFound(new { message = "No services in progress found." });

                return Ok(new { service });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Complete-Service/{serviceId}")]
        public async Task<IActionResult> CompleteService(string serviceId)
        {
            if (string.IsNullOrEmpty(serviceId))
                return BadRequest(new { message = "Invalid service data." });

            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork.providerServiceRepository.CompleteAsync(serviceId, userId);
                return Ok(new { message = "Service completed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
