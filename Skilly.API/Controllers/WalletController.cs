using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Persistence.Abstract;

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
       
    }
}
