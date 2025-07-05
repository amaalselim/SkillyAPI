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

        private string GetUserIdFromClaims()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authorized.");
            return userId;
        }

        [HttpGet("GetAllServiceProvider")]
        public async Task<IActionResult> GetAllServiceProvider()
        {
            try
            {
                var providers = await _unitOfWork.ServiceProviderRepository.GetAllServiceProviderAsync();
                return Ok(new { providers });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "An error occurred while retrieving service providers.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("GetServiceProviderByUserId")]
        public async Task<IActionResult> GetUserById()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var provider = await _unitOfWork.ServiceProviderRepository.GetByIdAsync(userId);
                if (provider == null)
                    return NotFound(new { message = "Provider not found." });

                return Ok(new { provider });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpGet("GetServiceProviderBy/{UserId}")]
        public async Task<IActionResult> GetUserById(string UserId)
        {
            try
            {
                var provider = await _unitOfWork.ServiceProviderRepository.GetproviderByIdAsync(UserId);
                if (provider == null)
                    return NotFound(new { message = "Provider not found." });

                return Ok(new { provider });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpGet("GetAllServiceProvidersBy/{categoryId}")]
        public async Task<IActionResult> GetserviceProviderbycategoryId(string categoryId)
        {
            try
            {
                var providers = await _unitOfWork.ServiceProviderRepository.GetAllserviceProvidersbyCategoryId(categoryId);
                if (providers == null || !providers.Any())
                    return NotFound(new { message = "No providers found for this category." });

                return Ok(new { providers });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpPost("addServiceProvider")]
        public async Task<IActionResult> AddServiceProvider([FromForm] ServiceProviderDTO dto)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                await _unitOfWork.ServiceProviderRepository.AddServiceProviderAsync(dto, userId);

                return Created("", new
                {
                    message = "Provider added successfully.",
                    data = dto
                });
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpPut("editServiceProvider")]
        public async Task<IActionResult> EditServiceProvider([FromForm] editServiceproviderDTO dto)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                await _unitOfWork.ServiceProviderRepository.EditServiceProviderAsync(dto, userId);

                return Ok(new
                {
                    message = "Provider updated successfully.",
                    data = dto
                });
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ServiceProviderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpDelete("deleteProviderByuserId")]
        public async Task<IActionResult> DeleteServiceProvider()
        {
            try
            {
                var id = GetUserIdFromClaims();

                await _unitOfWork.ServiceProviderRepository.DeleteServiceProviderAsync(id);

                return Ok(new { message = "Provider deleted successfully." });
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ServiceProviderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred.", error = ex.Message });
            }
        }
    }
}
