using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.Implementation;
using Skilly.Core.Enums;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IFirebaseAuthService _firebaseAuthService;

        public AuthController(IAuthService authService,IFirebaseAuthService firebaseAuthService)
        {
            _authService = authService;
            _firebaseAuthService = firebaseAuthService;
        }
        //[HttpPost("Select-UserType")]
        //public IActionResult SelectUserType([FromBody] UserTypeRequestDTO userTypeRequestDTO)
        //{
        //    if (!Enum.TryParse(userTypeRequestDTO.UserType.ToString(), out UserType userType) || !Enum.IsDefined(typeof(UserType), userType))
        //    {
        //        return BadRequest(new { Success = false, Message = "Invalid UserType." });
        //    }

        //    return Ok(new { Success = true, UserType = userType });
        //}

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            var result = await _authService.RegisterAsync(registerDTO);

            if (result.Succeeded)
            {
                return Ok(new { Success = true, Message = "User Registered Successfully. Please verify your email." });
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
            var isVerified = await _authService.VerifyEmailCodeAsync(verificationDTO);

            if (isVerified)
            {
                return Ok(new { Success = true, Message = "Email confirmed successfully." });
            }

            return BadRequest(new { Success = false, Message = "Invalid verification code." });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var response = await _authService.LoginAsync(loginDTO);

            return response != null
                ? Ok(response)
                : BadRequest(new { Success = false, Message = "Invalid login attempt." });
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ForgetPasswordDTO forgetPasswordDTO)
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
        public async Task<IActionResult> UpdatePassword(UpdatePasswordDTO updatePasswordDTO)
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
        [HttpPost("Login-Google")]
        public async Task<IActionResult> LoginGoogle([FromBody] TokenDto tokenDto)
        {
            if (string.IsNullOrEmpty(tokenDto.IdToken))
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "ID token cannot be null or empty.",
                    Errors = new[] { "ID token cannot be null or empty." }
                });
            }

            try
            {
                var firebaseUserInfo = await _firebaseAuthService.VerifyGoogleTokenAsync(tokenDto.IdToken);

                return Ok(new
                {
                    Success = true,
                    Message = "Token verified successfully.",
                    Data = firebaseUserInfo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Error verifying token.",
                    Errors = new[] { ex.Message }
                });
            }
        }


    }
}
