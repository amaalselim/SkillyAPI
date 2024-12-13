using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.Implementation;
using System.Runtime.CompilerServices;

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
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return Ok(new { users });
        }
        [HttpGet("GetUserBy/{id}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
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
        [HttpDelete("DeleteUserBy/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _unitOfWork.Users.DeleteAsync(id);
            return NoContent();
        }
    }
}
