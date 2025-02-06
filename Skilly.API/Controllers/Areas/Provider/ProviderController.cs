using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
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
                return Ok(new { providers });
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

        [HttpGet("GetServiceProviderBy/{id}")]
        public async Task<ActionResult<ServiceProvider>> GetUserById(string id)
        {
            var user = await _unitOfWork.ServiceProviderRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("GetAllServiceProvidersBy/{categoryId}")]
        public async Task<ActionResult<ServiceProvider>> GetserviceProviderbycategoryId(string categoryId)
        {
            var user = await _unitOfWork.ServiceProviderRepository.GetAllserviceProvidersbyCategoryId(categoryId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        } 
        [HttpPost("addServiceProvider")]
        [Authorize]
        public async Task<IActionResult> AddServiceProvider([FromForm] ServiceProviderDTO ServiceProviderDTO)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                await _unitOfWork.ServiceProviderRepository.AddServiceProviderAsync(ServiceProviderDTO, userId);

                return Ok(new
                {
                    message = "User profile added successfully.",
                    data = ServiceProviderDTO
                });
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }
        [HttpPut("editServiceProvider")]
        [Authorize]
        public async Task<IActionResult> EditServiceProvider([FromForm] ServiceProviderDTO ServiceProviderDTO)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                await _unitOfWork.ServiceProviderRepository.EditServiceProviderAsync(ServiceProviderDTO, userId);

                return Ok(new
                {
                    message = "User profile updated successfully.",
                    data = ServiceProviderDTO
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpDelete("deleteProviderBy/{id}")]
        public async Task<IActionResult> DeleteServiceProvider(string id)
        {
            try
            {

                await _unitOfWork.ServiceProviderRepository.DeleteServiceProviderAsync(id);

                return Ok(new { message = "User profile deleted successfully." });
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ServiceProviderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }


    }
}
