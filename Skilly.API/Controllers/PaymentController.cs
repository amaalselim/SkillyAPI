using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Skilly.Application.DTOs;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.Implementation;
using System.Security.Claims;
using System.Web;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentController(IUnitOfWork unitOfWork)
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
        [HttpPost("start-payment")]
        public async Task<IActionResult> StartPayment([FromBody] StartPaymentDTO paymentDTO)
        {
            {
                try
                {
                    var userId = GetUserIdFromClaims();
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Unauthorized(new { message = "User not authorized." });
                    }

                    var result = await _unitOfWork._paymentRepository.StartPaymentAsync(paymentDTO.ServiceId);
                    if (result == null)
                    {
                        return NotFound(new { message = "Payment initiation failed." });
                    }
                    return Ok(new { message = "Payment initiated successfully.", result });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

        }


        [HttpPost("start-payment-URL")]
        public async Task<IActionResult> StartPaymentURL([FromBody] StartPaymentURLDTO paymentDTO)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authorized." });
                }

                var result = await _unitOfWork._paymentRepository.StartPaymentAsync(paymentDTO.ServiceId, paymentDTO.RedirectUrl);
                if (result == null)
                {
                    return NotFound(new { message = "Payment initiation failed." });
                }
                return Ok(new { message = "Payment initiated successfully.", result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("payment-callback")]
        public async Task<IActionResult> PaymentCallbackCheck()
        {
            var orderId = HttpContext.Request.Query["order"].ToString();
            var successParam = HttpContext.Request.Query["success"].ToString();

            if (string.IsNullOrEmpty(orderId))
            {
                return BadRequest(new { success = false, message = "Order ID is required." });
            }
            if (!bool.TryParse(successParam, out bool success))
            {
                return BadRequest(new { success = false, message = "Invalid success value." });
            }

            if (!success)
            {
                return BadRequest(new { success = false, message = "Payment was not successful." });
            }

            var result = await _unitOfWork._paymentRepository.HandlePaymentCallbackAsync(orderId, success);
            if (result == null)
            {
                return NotFound(new { success = false, message = "Payment not found." });
            }

            return Ok(new { success = true, message = "Payment confirmed successfully." });
        }
        [HttpPost("payment-URl-callback/{orderId}")]
        public async Task<IActionResult> PaymentCallbackPost([FromQuery] string orderId, [FromBody] CallbackDTO callbackDTO)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return BadRequest(new { success = false, message = "Order ID is required." });
            }

            var result = await _unitOfWork._paymentRepository.HandlePaymentCallbackAsync(orderId, callbackDTO.success);
            if (result == null)
            {
                return NotFound(new { success = false, message = "Payment not found." });
            }

            return Ok(new { success = true, message = "Payment confirmed successfully." });
        }
        [HttpGet("GetAllTransactions")]
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                var trans = await _unitOfWork._paymentRepository.GetAllTransactions();

                if (trans == null || !trans.Any())
                {
                    return NotFound(new { message = "No Transactions found." });
                }
                return Ok(new { trans});
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

    }

}
