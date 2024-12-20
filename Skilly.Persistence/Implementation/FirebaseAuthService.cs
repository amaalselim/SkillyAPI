using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public FirebaseAuthService(ApplicationDbContext context,IConfiguration config)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential=GoogleCredential.FromFile("Configs/firebase-service-account.json")
            });
            _firebaseAuth= FirebaseAuth.DefaultInstance;
            _context = context;
            _config = config;
        }
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
                    JwtToken = tokenString
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
