using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using Skilly.Infrastructure.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public AuthService(IMapper mapper,
            UserManager<User> user,
            SignInManager<User> manager,
            ITokenService tokenService,
            IClaimsService claimsService
            )
        {
            _mapper = mapper;
            _usermanager = user;
            _manager = manager;
            _tokenService = tokenService;
            _claimsService = claimsService;
        }
        public async Task<object> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _usermanager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDTO.PhoneNumber);
            if (user != null)
            {
                var result = await _manager.PasswordSignInAsync(user.Email, loginDTO.Password, loginDTO.RememberMe, false);
                if (result.Succeeded)
                {
                    var claims = await _claimsService.GetClaimsAsync(loginDTO.PhoneNumber, user.Id);

                    var token = _tokenService.CreateTokenAsync(claims, loginDTO.RememberMe);

                    return new
                    {
                        Token = token,
                        Expire = loginDTO.RememberMe ? DateTime.Now.AddDays(30) : DateTime.Now.AddHours(20)
                    };
                }
            }
            return null;
        }


            public async Task<IdentityResult> RegisterAsync(RegisterDTO registerDTO)
            {
                var user = _mapper.Map<User>(registerDTO);
                return await _usermanager.CreateAsync(user,registerDTO.Password);
            }
    }
}
