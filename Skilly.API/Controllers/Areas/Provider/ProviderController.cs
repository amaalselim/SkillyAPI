using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs.Payment;
using Skilly.Application.DTOs.ServiceProvider;
using Skilly.Application.Exceptions;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using System.Security.Claims;
using ServiceProvider = Skilly.Core.Entities.ServiceProvider;

namespace Skilly.API.Controllers.Areas.Provider
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProviderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet("GetAllServiceProvider")]
        public async Task<ActionResult<IEnumerable<ServiceProvider>>> GetAllServiceProvider()
        {
            try
            {
                var providers = await _unitOfWork.ServiceProviderRepository.GetAllServiceProviderAsync();
                return StatusCode(200, new { providers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving service providers.",
                    error = ex.Message
                });
            }
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


        [HttpGet("GetServiceProviderByUserId")]
        public async Task<ActionResult<ServiceProvider>> GetUserById()
        {
            var userId = GetUserIdFromClaims();
            var provider = await _unitOfWork.ServiceProviderRepository.GetByIdAsync(userId);
            if (provider == null)
            {
                return StatusCode(404);
            }
            return StatusCode(200, new { provider });
        }

        [HttpGet("GetServiceProviderBy/{UserId}")]
        public async Task<ActionResult<ServiceProvider>> GetUserById(string UserId)
        {
            var provider = await _unitOfWork.ServiceProviderRepository.GetproviderByIdAsync(UserId);
            if (provider == null)
            {
                return StatusCode(404);
            }
            return StatusCode(200, new { provider });
        }

        [HttpGet("GetAllServiceProvidersBy/{categoryId}")]
        public async Task<ActionResult<ServiceProvider>> GetserviceProviderbycategoryId(string categoryId)
        {

            var provider = await _unitOfWork.ServiceProviderRepository.GetAllserviceProvidersbyCategoryId(categoryId);
            if (provider == null)
            {
                return StatusCode(404);
            }
            return StatusCode(200, new { provider });
        }

        [HttpPost("addServiceProvider")]
        public async Task<IActionResult> AddServiceProvider([FromForm] ServiceProviderDTO ServiceProviderDTO)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return StatusCode(401, new { message = "User not authenticated." });
                }

                await _unitOfWork.ServiceProviderRepository.AddServiceProviderAsync(ServiceProviderDTO, userId);

                return StatusCode(201, new
                {
                    message = "provider added successfully.",
                    data = ServiceProviderDTO
                });
            }
            catch (UserNotFoundException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpPut("editServiceProvider")]
        public async Task<IActionResult> EditServiceProvider([FromForm] editServiceproviderDTO ServiceProviderDTO)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return StatusCode(401, new { message = "User not authenticated." });
                }

                await _unitOfWork.ServiceProviderRepository.EditServiceProviderAsync(ServiceProviderDTO, userId);

                return StatusCode(200, new
                {
                    message = "Provider updated successfully.",
                    data = ServiceProviderDTO
                });
            }
            catch (UserNotFoundException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ServiceProviderNotFoundException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpDelete("deleteProviderByuserId")]
        public async Task<IActionResult> DeleteServiceProvider()
        {
            try
            {
                var id = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(id))
                {
                    return StatusCode(401, new { message = "User not authenticated." });
                }

                await _unitOfWork.ServiceProviderRepository.DeleteServiceProviderAsync(id);

                return StatusCode(200, new { message = "provider deleted successfully." });
            }
            catch (UserNotFoundException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (ServiceProviderNotFoundException ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

    }
}
