using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs.Payment;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

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
            throw new UnauthorizedAccessException("User not authorized.");
        return userId;
    }

    [HttpPost("start-payment")]
    [Authorize]
    public async Task<IActionResult> StartPayment([FromBody] StartPaymentDTO paymentDTO)
    {
        if (paymentDTO == null || string.IsNullOrEmpty(paymentDTO.ServiceId))
            return BadRequest(new { message = "Service ID is required." });

        var userId = GetUserIdFromClaims();
        var result = await _unitOfWork._paymentRepository.StartPaymentAsync(paymentDTO.ServiceId);

        if (result == null)
            return NotFound(new { message = "Payment initiation failed." });

        return Ok(new { message = "Payment initiated successfully.", result });
    }

    [HttpPost("start-payment-URL")]
    [Authorize]
    public async Task<IActionResult> StartPaymentURL([FromBody] StartPaymentURLDTO paymentDTO)
    {
        if (paymentDTO == null || string.IsNullOrEmpty(paymentDTO.ServiceId) || string.IsNullOrEmpty(paymentDTO.RedirectUrl))
            return BadRequest(new { message = "Invalid payment data." });

        var userId = GetUserIdFromClaims();
        var result = await _unitOfWork._paymentRepository.StartPaymentAsync(paymentDTO.ServiceId, paymentDTO.RedirectUrl);

        if (result == null)
            return NotFound(new { message = "Payment initiation failed." });

        return Ok(new { message = "Payment initiated successfully.", result });
    }

    [HttpGet("payment-callback")]
    public async Task<IActionResult> PaymentCallbackCheck()
    {
        var orderId = HttpContext.Request.Query["order"].ToString();
        var successParam = HttpContext.Request.Query["success"].ToString();

        if (string.IsNullOrEmpty(orderId))
            return BadRequest(new { success = false, message = "Order ID is required." });

        if (!bool.TryParse(successParam, out bool success))
            return BadRequest(new { success = false, message = "Invalid success value." });

        if (!success)
            return BadRequest(new { success = false, message = "Payment was not successful." });

        var result = await _unitOfWork._paymentRepository.HandlePaymentCallbackAsync(orderId, success);

        if (result.result == "Payment not found")
            return NotFound(new { success = false, message = "Payment not found." });

        return Ok(new
        {
            success = true,
            message = result.result,
            providerId = result.providerId,
            chatId = result.chatId
        });
    }

    [HttpPost("payment-URl-callback/{orderId}")]
    [Authorize]
    public async Task<IActionResult> PaymentCallbackPost(string orderId, [FromBody] CallbackDTO callbackDTO)
    {
        if (string.IsNullOrEmpty(orderId))
            return BadRequest(new { success = false, message = "Order ID is required." });

        if (!callbackDTO.success)
            return BadRequest(new { success = false, message = "Payment was not successful." });

        var result = await _unitOfWork._paymentRepository.HandlePaymentCallbackAsync(orderId, callbackDTO.success);

        if (result.result == "Payment not found")
            return NotFound(new { success = false, message = "Payment not found." });

        return Ok(new
        {
            success = true,
            message = result.result,
            providerId = result.providerId,
            chatId = result.chatId
        });
    }

    [HttpGet("GetAllTransactions")]
    public async Task<IActionResult> GetAllTransactions()
    {
        var trans = await _unitOfWork._paymentRepository.GetAllTransactions();

        if (trans == null || !trans.Any())
            return NotFound(new { message = "No Transactions found." });

        return Ok(new { trans });
    }

    [HttpGet("get-all-Transactions-by-providerId")]
    [Authorize]
    public async Task<IActionResult> GetAllTransactionsbyProviderId()
    {
        var providerId = GetUserIdFromClaims();
        var trans = await _unitOfWork._paymentRepository.GetTransactionsGroupedByDate(providerId);

        if (trans == null || !trans.Any())
            return NotFound(new { message = "No Transactions found." });

        return Ok(new { trans });
    }
}
