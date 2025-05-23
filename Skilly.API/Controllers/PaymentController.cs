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
        //[HttpGet("payment-callback")]
        //public async Task<IActionResult> HandlePaymentCallback(string order, string url)
        //{
        //    if (string.IsNullOrEmpty(order))
        //    {
        //        return BadRequest(new { success = false, message = "Order ID is missing." });
        //    }

        //    var result = await _unitOfWork._paymentRepository.HandlePaymentCallbackAsync(order);

        //    if (result == null)
        //    {
        //        return NotFound(new { success = false, message = "Payment not found." });
        //    }

        //    return Redirect($"{url}");
        //}

        //[HttpPost("payment-callback")]
        //public async Task<IActionResult> PaymentCallback([FromBody] PaymobCallbackDTO callbackData)
        //{
        //    if (callbackData == null || string.IsNullOrEmpty(callbackData.OrderId) || string.IsNullOrEmpty(callbackData.Url))
        //    {
        //        return BadRequest(new
        //        {
        //            url = string.IsNullOrEmpty(callbackData?.Url) ? new[] { "The url field is required." } : null,
        //            order = string.IsNullOrEmpty(callbackData?.OrderId) ? new[] { "The order field is required." } : null,
        //        });
        //    }

        //    var success = callbackData.Success;

        //    if (!success)
        //    {
        //        return BadRequest(new { success = false, message = "Payment was not successful." });
        //    }

        //    var result = await _unitOfWork._paymentRepository.HandlePaymentCallbackAsync(callbackData.OrderId, success);
        //    if (result == null)
        //    {
        //        return NotFound(new { success = false, message = "Payment not found." });
        //    }

        //    var uriBuilder = new UriBuilder(callbackData.Url);
        //    var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        //    query["order"] = callbackData.OrderId;
        //    uriBuilder.Query = query.ToString();

        //    string fullUrl = uriBuilder.ToString();

        //    return Ok(new { success = true, endpointfullPath = fullUrl });
        //}

        //[HttpGet("payment-callback")]
        //public async Task<IActionResult> PaymentCallback([FromQuery] bool success, [FromQuery] string order)
        //{
        //    if (string.IsNullOrEmpty(order))
        //    {
        //        return BadRequest(new { success = false, message = "Order ID is required." });
        //    }

        //    if (!success)
        //    {
        //        return BadRequest(new { success = false, message = "Payment was not successful." });
        //    }

        //    var result = await _unitOfWork._paymentRepository.HandlePaymentCallbackAsync(order, success);
        //    if (result == null)
        //    {
        //        return NotFound(new { success = false, message = "Payment not found." });
        //    }

        //    return Ok(new { success = true, message = "Payment confirmed successfully." });
        //}

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





    }

}
