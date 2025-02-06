
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using Skilly.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.Implementation
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly FirebaseAuth _firebaseAuth;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<FirebaseAuthService> _logger;
        private readonly Random _random;
        public FirebaseAuthService(ApplicationDbContext context,IConfiguration config, ILogger<FirebaseAuthService> logger)
        {
            //FirebaseApp.Create(new AppOptions()
            //{
            //    Credential=GoogleCredential.FromFile("Configs/firebase-service-account.json")
            //});
            _firebaseAuth= FirebaseAuth.DefaultInstance;
            _context = context;
            _config = config;
            _logger = logger;
            _random = new Random();
        }


        //public async Task<bool> VerifyOtpAsync(string phoneNumber, int otpCode)
        //{
        //    try
        //    {
        //        // Use Firebase's Phone Auth Provider to verify the OTP code
        //        var phoneAuthProvider = PhoneAuthProvider.GetInstance(FirebaseAuth.DefaultInstance);

        //        // Assuming OTP verification code is valid when sent from Flutter
        //        var verificationResult = await phoneAuthProvider.VerifyPhoneNumberAsync(phoneNumber, otpCode);

        //        // If successful verification result, return true
        //        return verificationResult.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"OTP verification failed for phone {phoneNumber}: {ex.Message}");
        //        return false;
        //    }
        //}
        public async Task<FirebaseUserInfoDTO> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                var token = await _firebaseAuth.VerifyIdTokenAsync(idToken);
                var user = await _firebaseAuth.GetUserAsync(token.Uid);

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Uid);
                if (existingUser == null)
                {
                    var newUser = new User
                    {
                        Id = user.Uid,
                        Email = user.Email,
                        UserName = user.DisplayName
                    };
                    await _context.Users.AddAsync(newUser);
                    await _context.SaveChangesAsync();
                }
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Uid),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiration = DateTime.Now.AddDays(30);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = new JwtSecurityToken(
                    issuer: _config["JWT:Issuer"],   
                    audience: _config["JWT:Audience"],
                    claims: claims,
                    expires: expiration,
                    signingCredentials: creds
                );

                var tokenString = tokenHandler.WriteToken(jwtToken);

                var firebaseUserInfo = new FirebaseUserInfoDTO
                {
                    Uid = user.Uid,
                    Email = user.Email,
                    Name = user.DisplayName,
                    JwtToken = tokenString,
                    Expiration = expiration
                };
                return firebaseUserInfo;
            }
            catch (FirebaseAuthException ex)
            {
                throw new Exception("Failed to verify Google token: " + ex.Message);
            }
        }
    }
}
