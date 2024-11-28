using Microsoft.AspNetCore.Identity;
using Skilly.Application.Abstract;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Infrastructure.Implementation
{
    public class ClaimsService : IClaimsService
    {
        private readonly UserManager<User> _userManager;

        public ClaimsService(UserManager<User> userManager)
        {
           _userManager = userManager;
        }
        public async Task<List<Claim>> GetClaimsAsync(string PhoneNumber, string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.MobilePhone, PhoneNumber),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            return claims;
        }
    }
}
