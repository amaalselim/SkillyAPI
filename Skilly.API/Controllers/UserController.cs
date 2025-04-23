using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.Implementation;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            
            var users = await _unitOfWork.Users.GetAllAsync();
            return Ok(new { users });
        }
        [HttpGet("GetUserById}")]
        public async Task<ActionResult<User>> GetUserById()
        {
            var userid = GetUserIdFromClaims();
            var user = await _unitOfWork.Users.GetByIdAsync(userid);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new { user });
        }
        [HttpPut("EditUserBy/{id}")]
        public async Task<IActionResult> EditUser(string id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            await _unitOfWork.Users.UpdateAsync(user);

            return NoContent();
        }
        [HttpDelete("DeleteUserById")]
        public async Task<IActionResult> DeleteUser()
        {
            var userId= GetUserIdFromClaims();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            await _unitOfWork.Users.DeleteAsync(userId);
            return NoContent();
        }
    }
}
