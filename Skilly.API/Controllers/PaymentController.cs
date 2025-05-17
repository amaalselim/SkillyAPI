using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Skilly.Application.DTOs;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.Implementation;
using System.Security.Claims;

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

        [HttpGet("payment-callback")]
        public async Task<IActionResult> HandlePaymentCallback()
        {
            

            var orderId = Request.Query["order"].ToString();

            if (string.IsNullOrEmpty(orderId))
            {
                return BadRequest(new { message = "Payment ID is missing in the query parameters." });
            }

            var result = await _unitOfWork._paymentRepository.HandlePaymentCallbackAsync(orderId);

            if (result == null)
            {
                return NotFound(new { message = "Payment not found." });
            }

            return Redirect("https://skilly.runasp.net/thank-you.html");
        }
        //[HttpGet("payment-callback")]
        //public async Task<IActionResult> HandlePaymentCallback()
        //{
        //    var orderId = Request.Query["order"].ToString();

        //    if (string.IsNullOrEmpty(orderId))
        //    {
        //        return BadRequest(new { success = false, message = "Order ID is missing." });
        //    }

        //    var result = await _unitOfWork._paymentRepository.HandlePaymentCallbackAsync(orderId);

        //    if (result == null)
        //    {
        //        return NotFound(new { success = false, message = "Payment not found." });
        //    }

        //    return Ok(new { success = true });
        //}





    }
}
