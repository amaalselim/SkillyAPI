using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.Implementation;
using Skilly.Core.Entities;
using Skilly.Core.Enums;
using Skilly.Persistence.Abstract;
using System.Security.Claims;
using Vonage.Common;

namespace Skilly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IGenericRepository<User> _user;

        public AuthController(IAuthService authService,IGenericRepository<User> user)
        {
            _authService = authService;
            _user = user;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var result = await _authService.RegisterAsync(registerDTO);

            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(Register), new { message = "User Registered Successfully. Please verify your email." });
            }

            return BadRequest(new
            {
                Success = false,
                Message = "Registration failed.",
                Errors = result.Errors.Select(e => e.Description)
            });
        }


        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerficationCodeDTO verificationDTO)
        {
            var token = await _authService.VerifyEmailCodeAsync(verificationDTO);

            if (token != null)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Email confirmed successfully.",
                    Token = token
                });
            }
            return BadRequest(new
            {
                Success = false,
                Message = "Invalid verification code."
            });
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid model",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var response = await _authService.LoginAsync(loginDTO);
            return response != null ? Ok(response): BadRequest(new { Success = false, Message = "Invalid login attempt." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ForgetPasswordDTO forgetPasswordDTO)
        {
            var token = await _authService.GeneratePasswordResetTokenAsync(forgetPasswordDTO);
            if (token == null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to generate reset token."
                });
            }
            try
            {
                await _authService.SendResetPasswordEmailAsync(forgetPasswordDTO.Email);
                return Ok(new
                {
                    Success = true,
                    Message = "Reset password email sent successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Failed to send email: {ex.Message}"
                });
            }
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerficationCodeDTO verficationCodeDTO)
        {
            var user = await _authService.FindByEmailAsync(verficationCodeDTO.email);
            if (user == null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            if (user.verificationCode.ToString() != verficationCodeDTO.code)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid verification code."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Verification successful."
            });
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO updatePasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid data provided.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var result = await _authService.UpdatePasswordAsync(updatePasswordDTO);
            if (result.Succeeded)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Password updated successfully."
                });
            }
            else
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Password update failed.",
                    Errors = result.Errors
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
        [HttpPost("Addlocation")]
        public async Task<IActionResult> SaveUserLocation([FromBody] LocationDTO location)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { status = "error", message = "User not authorized." });

            var user = await _user.GetByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            user.Latitude = location.Latitude;
            user.Longitude = location.Longitude;

            await _user.UpdateAsync(user);

            return Ok(new { message = "Location saved successfully." });
        }
    }
}
