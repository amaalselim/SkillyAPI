using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.Implementation;
using Skilly.Core.Enums;
using Vonage.Common;

namespace Skilly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
                    Success = true, Message = "Email confirmed successfully.",Token = token
                });
            }
            return BadRequest(new
            {
                Success = false,Message = "Invalid verification code."
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

        //[HttpPost("Login-Google")]
        //public async Task<IActionResult> LoginGoogle([FromBody] TokenDto tokenDto)
        //{
        //    if (string.IsNullOrEmpty(tokenDto.IdToken))
        //    {
        //        return BadRequest(new
        //        {
        //            Success = false,
        //            Message = "ID token cannot be null or empty.",
        //            Errors = new[] { "ID token cannot be null or empty." }
        //        });
        //    }

        //    try
        //    {
        //        var firebaseUserInfo = await _firebaseAuthService.VerifyGoogleTokenAsync(tokenDto.IdToken);

        //        return Ok(new
        //        {
        //            Success = true,
        //            Message = "Token verified successfully.",
        //            Data = firebaseUserInfo
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new
        //        {
        //            Success = false,
        //            Message = "Error verifying token.",
        //            Errors = new[] { ex.Message }
        //        });
        //    }
        //}

        //[HttpPost("verify-otp")]
        //public async Task<IActionResult> Verifyotp([FromBody]VerifyOtpDTO verifyOtpDTO)
        //{
        //    if (verifyOtpDTO.PhoneNumber == null || verifyOtpDTO.otpCode==null)
        //    {
        //        return BadRequest(new
        //        {
        //            Success = false,
        //            Message = "Phone number and OTP code are required!",
        //            Errors = new[] { "Both phone number and OTP code are mandatory." }
        //        });
        //    }
        //    var isvalid= await _firebaseAuthService.VerifyOtpAsync(verifyOtpDTO.PhoneNumber,verifyOtpDTO.otpCode);
        //    if (!isvalid)
        //    {
        //        return BadRequest(new
        //        {
        //            Success=false,
        //            Message="Invalid OTP!",
        //            Errors= new[] { "The OTP you entered is incorrect." }
        //        });
        //    }
        //    return Ok(new
        //    {
        //        Success = true,
        //        Message = "OTP verified successfully!",
        //        Data = new { PhoneNumber = verifyOtpDTO.PhoneNumber }
        //    });
        //}
    }
}
