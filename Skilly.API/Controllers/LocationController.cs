using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using System.Security.Claims;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly IGenericRepository<User> _user;

        public LocationController(IGenericRepository<User> user)
        {
            _user = user;
        }
        private string GetUserIdFromClaims()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authorized.");
            }
            return userId;
        }

        [HttpPost("Addlocation")]
        public async Task<IActionResult> SaveUserLocation([FromBody] LocationDTO location)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { status = "error", message = "User not authorized." });

            var user= await _user.GetByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            user.Latitude = location.Latitude;
            user.Longitude = location.Longitude;

             await _user.UpdateAsync(user);

            return Ok(new { message = "Location saved successfully." });
        }
    }
}
