using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            var result = await _authService.RegisterAsync(registerDTO);
            if (result.Succeeded)
            {
                return Ok(new { Success = true, Message = "User Registered Successfully." });
            }
            return BadRequest(new { Success = false, Message = "Registration failed.", Errors = result.Errors });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var user = await _authService.LoginAsync(loginDTO);

            if (user == null)
            {
                return BadRequest(new { Success = false, Message = "Invalid Login Attempt." });
            }

            return Ok(new { Success = true, Message = "Login successful.", User = user });
        }

    }
}
