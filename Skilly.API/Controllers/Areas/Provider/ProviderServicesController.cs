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
        public async Task<IActionResult> GetAllServices()
        {
            try
            {
                //string userId = GetUserIdFromClaims();
                var Services = await _unitOfWork.providerServiceRepository.GetAllProviderService();
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
        [HttpGet("GetAllServicesByproviderId")]
        public async Task<ActionResult<ProviderServices>> GetservicesbyuserId()
        {
            string userId = GetUserIdFromClaims();
            var service = await _unitOfWork.providerServiceRepository.GetAllServicesByproviderId(userId);
            if (service == null)
            {
                return NotFound();
            }
            return Ok(new { service });
        }
        [HttpGet("GetAllServicesBy/{categoryId}")]
        public async Task<ActionResult<ProviderServices>> GetservicesbycategoryId(string categoryId)
        {
            var service = await _unitOfWork.providerServiceRepository.GetAllservicesbyCategoryId(categoryId);
            if (service == null)
            {
                return NotFound();
            }
            return Ok(new { service });
        }


        [HttpGet("GeyServiceBy/{serviceId}")]
        public async Task<IActionResult> GetServiceById([FromRoute] string serviceId)
        {
            try
            {
                string userId = GetUserIdFromClaims();
                var service = await _unitOfWork.providerServiceRepository.GetProviderServiceByIdAsync(serviceId, userId);

                return Ok(new { service });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        
        [HttpPost("AddService")]
        public async Task<IActionResult> AddService([FromForm] ProviderservicesDTO providerservicesDTO)
        {
            if (providerservicesDTO == null)
            {
                return BadRequest(new { message = "Invalid service data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.providerServiceRepository.AddProviderService(providerservicesDTO, userId);

                return Ok(new { message = "Service added successfully.", data = providerservicesDTO });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpPut("EditServiceBy/{serviceId}")]
        public async Task<IActionResult> EditService([FromForm] ProviderservicesDTO providerservicesDTO, [FromRoute] string serviceId)
        {
            if (providerservicesDTO == null)
            {
                return BadRequest(new { message = "Invalid service data." });
            }

            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.providerServiceRepository.EditProviderService(providerservicesDTO, userId, serviceId);

                return Ok(new { message = "Service updated successfully.", data = providerservicesDTO });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpDelete("DeleteServiceBy/{serviceId}")]
        public async Task<IActionResult> DeleteService([FromRoute] string serviceId)
        {
            try
            {
                string userId = GetUserIdFromClaims();
                await _unitOfWork.providerServiceRepository.DeleteProviderServiceAsync(serviceId, userId);

                return Ok(new { message = "Service  deleted successfully." });
            }
            catch (ProviderServiceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
