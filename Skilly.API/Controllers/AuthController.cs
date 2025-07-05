using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.DTOs.Auth;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

namespace Skilly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IGenericRepository<User> _user;

        public AuthController(IAuthService authService, IGenericRepository<User> user)
        {
            _authService = authService;
            _user = user;
        }

        private string GetUserIdFromClaims()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authorized.");
            return userId;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var result = await _authService.RegisterAsync(registerDTO);
            if (result.Succeeded)
                return CreatedAtAction(nameof(Register), new { message = "User registered successfully. Please verify your email." });

            return BadRequest(new
            {
                Success = false,
                Message = "Registration failed.",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerficationCodeDTO dto)
        {
            var token = await _authService.VerifyEmailCodeAsync(dto);
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
                    Message = "Invalid model.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var response = await _authService.LoginAsync(loginDTO);
            return response != null
                ? Ok(response)
                : BadRequest(new { Success = false, Message = "Invalid login attempt." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ForgetPasswordDTO dto)
        {
            var token = await _authService.GeneratePasswordResetTokenAsync(dto);
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
                await _authService.SendResetPasswordEmailAsync(dto.Email);
                return Ok(new
                {
                    Success = true,
                    Message = "Reset password email sent successfully."
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = $"Failed to send email: {ex.Message}"
                });
            }
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerficationCodeDTO dto)
        {
            var user = await _authService.FindByEmailAsync(dto.email);
            if (user == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "User not found."
                });
            }

            if (user.verificationCode.ToString() != dto.code)
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
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var result = await _authService.UpdatePasswordAsync(dto);
            return result.Succeeded
                ? Ok(new { Success = true, Message = "Password updated successfully." })
                : BadRequest(new { Success = false, Message = "Password update failed.", Errors = result.Errors });
        }

        [HttpPost("login-google")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] LoginGoogleDTO dto)
        {
            var result = await _authService.LoginWithGoogleAsync(dto);
            return result != null
                ? Ok(result)
                : BadRequest(new { message = "Invalid Google login." });
        }

        [HttpPost("AddLocation")]
        public async Task<IActionResult> SaveUserLocation([FromBody] LocationDTO location)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserIdFromClaims();
            var user = await _user.GetByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            user.Latitude = location.Latitude;
            user.Longitude = location.Longitude;

            await _user.UpdateAsync(user);

            return Ok(new { message = "Location saved successfully." });
        }

        [HttpPost("CompleteGoogleData")]
        public async Task<IActionResult> CompleteGoogleData([FromBody] CompleteGoogleDataDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Invalid data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            try
            {
                await _authService.CompleteDataAsync(dto);
                return Ok(new
                {
                    Success = true,
                    Message = "Google data completed successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Error while completing Google data: {ex.Message}"
                });
            }
        }
    }
}
