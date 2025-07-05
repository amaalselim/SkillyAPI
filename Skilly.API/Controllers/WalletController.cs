using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs.Payment;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class WalletController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public WalletController(IUnitOfWork unitOfWork)
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

    [HttpGet("get-all-balance")]
    [Authorize]
    public async Task<IActionResult> GetWalletBalance()
    {
        try
        {
            var balance = await _unitOfWork._paymentRepository.GetWalletsAsync();

            if (balance == null || !balance.Any())
                return NotFound(new { message = "No wallet data found." });

            return Ok(new { status = "success", balance });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpGet("get-balance")]
    [Authorize]
    public async Task<IActionResult> ProcessPayment()
    {
        try
        {
            var providerId = GetUserIdFromClaims();

            var result = await _unitOfWork._paymentRepository.ProcessPaymentAsync(providerId);
            if (result == null)
                return NotFound(new { message = "No balance found for this user." });

            return Ok(new { status = "success", result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Unexpected error occurred.", error = ex.Message });
        }
    }

    [HttpPost("apply-withdraw")]
    [Authorize]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequestDTO request)
    {
        if (request == null || request.Amount <= 0)
            return BadRequest(new { message = "Invalid withdrawal request." });

        try
        {
            var providerId = GetUserIdFromClaims();
            request.ProviderId = providerId;

            var result = await _unitOfWork._paymentRepository.RequestWithdrawAsync(request);

            if (string.IsNullOrEmpty(result))
                return BadRequest(new { message = "Withdraw request failed." });

            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Unexpected error occurred.", error = ex.Message });
        }
    }
}
