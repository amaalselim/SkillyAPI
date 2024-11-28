using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Skilly.Application.Abstract;
using Skilly.Infrastructure.Abstract;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Infrastructure.Implementation
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config )
        {
            _config = config;
        }
        public async Task<string> CreateTokenAsync(IEnumerable<Claim> claims, bool rememberMe)
        {
            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenExpiration = rememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddHours(20);
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                expires: tokenExpiration,
                claims: claims,
                signingCredentials: signingCredentials
            );

            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}
