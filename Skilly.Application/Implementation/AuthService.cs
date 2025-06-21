using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.DTOs.Auth;
using Skilly.Core.Entities;
using Skilly.Infrastructure.Abstract;
using Skilly.Infrastructure.Implementation;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _usermanager;
        private readonly SignInManager<User> _manager;
        private readonly ITokenService _tokenService;
        private readonly IClaimsService _claimsService;

        public IHttpContextAccessor _httpContextAccessor { get; }
        public IEmailService _emailService { get; }

        public AuthService(IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> user,
            SignInManager<User> manager,
            ITokenService tokenService,
            IClaimsService claimsService,
            IEmailService emailService
            )
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _usermanager = user;
            _manager = manager;
            _tokenService = tokenService;
            _claimsService = claimsService;
            _emailService = emailService;
        }
        public async Task<object> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _usermanager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDTO.PhoneNumber);

            if (user != null)
            {
                if (!user.EmailConfirmed)
                {
                    return new
                    {
                        Success = false,
                        Message = "Email not confirmed. Please verify your email first."
                    };
                }
                var result = await _manager.PasswordSignInAsync(user.Email, loginDTO.Password, loginDTO.RememberMe, false);
                if (result.Succeeded)
                {
                    var claims = await _claimsService.GetClaimsAsync(loginDTO.PhoneNumber, user.Id);

                    var token = _tokenService.CreateTokenAsync(claims, loginDTO.RememberMe);

                    return new
                    {
                        Success = true,
                        Message = "Login successful.",
                        Token = token.Result,
                        UserType = user.UserType.ToString(),
                        Expire = loginDTO.RememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddHours(20)
                    };
                }
                return new { Success = false, Message = "Invalid login credentials." };
            }
            return new { Success = false, Message = "User not found." };
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDTO registerDTO)
        {
            var existingUser = await _usermanager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == registerDTO.PhoneNumber);
            if (existingUser != null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicatePhoneNumber",
                    Description = "This phone number is already registered."
                });
            }

            var user = _mapper.Map<User>(registerDTO);
            user.UserType = registerDTO.UserType;
            user.FcmToken = registerDTO.FcmToken;
            user.EmailConfirmed = false;

            var result = await _usermanager.CreateAsync(user, registerDTO.Password);
            if (result.Succeeded)
            {
                Random random = new Random();
                int verificationCode = random.Next(1000, 9999);
                user.verificationCode = verificationCode;
                var updateResult = await _usermanager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return updateResult;
                }
                try
                {
                    await _emailService.SendEmailAsync(
                        user.Email,
                        user.FirstName,
                        "Email Confirmation",
                        $"Your email verification code is:<br/><code style='font-size: 18px; color: #3498db;'>{verificationCode}</code>"
                    );
                }
                catch (Exception ex)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = $"Failed to send email: {ex.Message}"
                    });
                }
            }
            return result;
        }
        public async Task<string> GeneratePasswordResetTokenAsync(ForgetPasswordDTO forgetPasswordDTO)
        {
            var user = await _usermanager.FindByEmailAsync(forgetPasswordDTO.Email);
            if (user == null)
            {
                return null;
            }
            return await _usermanager.GeneratePasswordResetTokenAsync(user);
        }
        public async Task SendResetPasswordEmailAsync(string email)
        {
            var user = await _usermanager.FindByEmailAsync(email);
            if (user == null)
            {
                return;
            }
            Random random = new Random();
            int verificationCode = random.Next(1000, 9999);

            user.verificationCode = verificationCode;
            await _usermanager.UpdateAsync(user);
            await _emailService.SendEmailAsync(email, user.FirstName, "Reset Your Password",
            $"Your verification code is:<br/><code style='font-size: 18px; color: #3498db;'>{verificationCode}</code>");
        }
        public async Task<IdentityResult> UpdatePasswordAsync(UpdatePasswordDTO updatePasswordDTO)
        {
            var user = await _usermanager.FindByEmailAsync(updatePasswordDTO.Email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }
            var token = await _usermanager.GeneratePasswordResetTokenAsync(user);
            var result = await _usermanager.ResetPasswordAsync(user, token, updatePasswordDTO.NewPassword);

            if (result.Succeeded)
            {
                user.verificationCode = null;
                await _usermanager.UpdateAsync(user);
            }
            return result;
        }
        public async Task<User> FindByEmailAsync(string email)
        {
            return await _usermanager.FindByEmailAsync(email);
        }
        public async Task<string> VerifyEmailCodeAsync(VerficationCodeDTO verficationCodeDTO)
        {
            var user = await _usermanager.FindByEmailAsync(verficationCodeDTO.email);
            if (user == null)
            {
                return null;
            }
            var claims = await _claimsService.GetClaimsAsync(user.PhoneNumber, user.Id);

            var token = _tokenService.CreateTokenAsync(claims, false);
            if (user.verificationCode.ToString() == verficationCodeDTO.code)
            {
                user.EmailConfirmed = true;
                user.verificationCode = null;
                await _usermanager.UpdateAsync(user);
                return token.Result;
            }

            return null;
        }

        public async Task<object> LoginWithGoogleAsync(LoginGoogleDTO googleLoginDTO)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleLoginDTO.IdToken);
            if (payload == null)
                throw new Exception("Invalid Google ID Token");

            var user = await _usermanager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new User
                {
                    FirstName = payload.Name,
                    LastName = payload.FamilyName,
                    UserName = payload.Email,
                    Email = payload.Email,
                    PhoneNumber = payload.Email
                };

                var createResult = await _usermanager.CreateAsync(user);
                if (!createResult.Succeeded)
                    throw new Exception("Failed to create user");
            }

            var claims = await _claimsService.GetClaimsAsync2(user.Email, user.Id);
            var token = await _tokenService.CreateTokenAsync(claims, false);

            return new
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                Expire = DateTime.Now.AddHours(20)
            };
        }
        public async Task CompleteDataAsync(CompleteGoogleDataDTO completeGoogleDataDTO)
        {
            var user = await _usermanager.FindByEmailAsync(completeGoogleDataDTO.email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            user.PhoneNumber = completeGoogleDataDTO.PhoneNumber;
            user.UserType = completeGoogleDataDTO.UserType;
            user.FcmToken = completeGoogleDataDTO.FcmToken;
            var result = await _usermanager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to update user data.");
            }
        }
    }
}
