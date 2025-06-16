using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs.Payment;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

namespace Skilly.API.Controllers
{
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
            {
                throw new UnauthorizedAccessException("User not authorized.");
            }
            return userId;
        }

        [HttpPost("provider-wallet/{paymentId}")]
        [Authorize]
        public async Task<IActionResult> ProcessPayment(string paymentId)
        {
            try
            {
                var result = await _unitOfWork._paymentRepository.ProcessPaymentAsync(paymentId);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("apply-withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequestDTO request)
        {
            try
            {
                var providerId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(providerId))
                {
                    return Unauthorized(new { message = "User not authorized." });
                }
                request.ProviderId = providerId;
                var result = await _unitOfWork._paymentRepository.RequestWithdrawAsync(request);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



    }
}
