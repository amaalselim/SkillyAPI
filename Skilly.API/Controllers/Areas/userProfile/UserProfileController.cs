using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.Exceptions;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.Implementation;
using System.Security.Claims;

namespace Skilly.API.Controllers.Areas.userProfile
{
    [Route("api/[controller]")]
    [ApiController]
    public class userProfileController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public userProfileController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet("GetAllUsersProfile")]
        public async Task<ActionResult<IEnumerable<UserProfile>>> GetAllUserProfile()
        {
            var users = await _unitOfWork.ProfileRepository.GetAllUserProfileAsync();
            return Ok(users);
        }
        [HttpGet("GetUserProfileBy/{id}")]
        public async Task<ActionResult<UserProfile>> GetUserById(string id)
        {
            var user = await _unitOfWork.ProfileRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        [HttpPost("addUserProfile")]
        [Authorize]
        public async Task<IActionResult> AddUserProfile([FromForm] UserProfileDTO UserProfileDTO)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                await _unitOfWork.ProfileRepository.AddUserProfileAsync(UserProfileDTO, userId);

                return Ok(new
                {
                    message = "User profile added successfully.",
                    data = UserProfileDTO
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
        [Authorize]
        public async Task<IActionResult> EditUserProfile([FromForm] UserProfileDTO UserProfileDTO)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                await _unitOfWork.ProfileRepository.EditUserProfileAsync(UserProfileDTO, userId);

                return Ok(new
                {
                    message = "User profile updated successfully.",
                    data = UserProfileDTO
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
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpDelete("deleteProfileBy/{id}")]
        public async Task<IActionResult> DeleteUserProfile(string id)
        {
            try
            {

                await _unitOfWork.ProfileRepository.DeleteUserProfileAsync(id);

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
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }
    }
}
