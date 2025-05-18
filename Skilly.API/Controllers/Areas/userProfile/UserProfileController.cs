using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
    public class UserProfileController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserProfileController(IUnitOfWork unitOfWork)
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

        [HttpGet("GetAllUsersProfile")]
        public async Task<ActionResult<IEnumerable<UserProfile>>> GetAllUserProfile()
        {
            try
            {
                var users = await _unitOfWork.ProfileRepository.GetAllUserProfileAsync();
                if (users == null || !users.Any())
                {
                    return NotFound(new { message = "No user profiles found." });
                }
                return Ok(new { users }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" }); 
            }
        }

        [HttpGet("GetUserProfileByuserId")]
        public async Task<ActionResult<UserProfile>> GetUserById()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                var user = await _unitOfWork.ProfileRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User profile not found." }); 
                }
                return Ok(new { user }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" }); 
            }
        }

        [HttpPost("addUserProfile")]
        public async Task<IActionResult> AddUserProfile([FromForm] UserProfileDTO userProfileDTO)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                await _unitOfWork.ProfileRepository.AddUserProfileAsync(userProfileDTO, userId);

                return CreatedAtAction(nameof(GetUserById), new { userId = userId }, new
                {
                    message = "User profile added successfully.",
                    data = userProfileDTO
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

        [HttpPut("editUserProfile")]
        public async Task<IActionResult> EditUserProfile([FromForm] UserProfileDTO userProfileDTO)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated." }); 
                }

                await _unitOfWork.ProfileRepository.EditUserProfileAsync(userProfileDTO, userId);

                return Ok(new
                {
                    message = "User profile updated successfully.",
                    data = userProfileDTO
                }); 
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); 
            }
            catch (UserProfileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message }); // 500 Internal Server Error
            }
        }

        [HttpDelete("deleteProfileByuserId")]
        public async Task<IActionResult> DeleteUserProfile()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork.ProfileRepository.DeleteUserProfileAsync(userId);

                return Ok(new { message = "User profile deleted successfully." }); 
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); 
            }
            catch (UserProfileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message }); // 500 Internal Server Error
            }
        }
    }
}
