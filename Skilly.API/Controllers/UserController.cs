using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

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
            throw new UnauthorizedAccessException("User not authorized.");
        return userId;
    }

    [HttpGet("GetAllUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        if (users == null || !users.Any())
            return NotFound(new { message = "No users found." });

        return Ok(new { users });
    }

    [HttpGet("GetUserById")]
    [Authorize]
    public async Task<IActionResult> GetUserById()
    {
        var userId = GetUserIdFromClaims();
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found." });

        return Ok(new { user });
    }

    [HttpPut("EditUserBy/{id}")]
    public async Task<IActionResult> EditUser(string id, [FromBody] User user)
    {
        if (string.IsNullOrEmpty(id) || user == null)
            return BadRequest(new { message = "Invalid user data." });

        if (id != user.Id)
            return BadRequest(new { message = "User ID mismatch." });

        var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
        if (existingUser == null)
            return NotFound(new { message = "User not found." });

        await _unitOfWork.Users.UpdateAsync(user);
        return Ok(new { message = "User updated successfully." });
    }

    [HttpDelete("DeleteUserBy/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { message = "User ID is required." });

        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found." });

        await _unitOfWork.Users.DeleteAsync(id);
        return Ok(new { message = "User deleted successfully." });
    }

    [HttpDelete("DeleteUserById")]
    [Authorize]
    public async Task<IActionResult> DeleteUserByToken()
    {
        var userId = GetUserIdFromClaims();

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        await _unitOfWork.Users.DeleteAsync(userId);
        return Ok(new { message = "User deleted successfully." });
    }
}
