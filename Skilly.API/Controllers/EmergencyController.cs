using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs.Emergency;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmergencyController : ControllerBase
    {
        private readonly IEmergencyService _emergencyService;
        private readonly IUnitOfWork _unitOfWork;

        public EmergencyController(IEmergencyService emergencyService, IUnitOfWork unitOfWork)
        {
            _emergencyService = emergencyService;
            _unitOfWork = unitOfWork;
        }

        private string GetUserIdFromClaims()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authorized.");
            return userId;
        }

        [HttpPost("create-emergency-request")]
        [Authorize]
        public async Task<IActionResult> CreateEmergencyRequest([FromBody] EmergencyRequestDTO emergencyRequestDTO)
        {
            var userId = GetUserIdFromClaims();

            if (emergencyRequestDTO == null)
                return BadRequest(new { status = "error", message = "Emergency request data is required." });

            try
            {
                var requestId = await _emergencyService.CreateEmergencyRequestAsync(emergencyRequestDTO, userId);
                return Ok(new
                {
                    status = "success",
                    message = "Emergency request created successfully.",
                    data = new { requestId }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("SendAvailableproviders")]
        public async Task<IActionResult> SendAvailableproviders([FromBody] emergencyDashboardDTO request)
        {
            try
            {
                await _emergencyService.SendEmergencyToDashboardbyId(request.emergencyId, request.price);
                return Ok(new { status = "success", message = "Emergency requests sent to dashboard successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("nearby-providers/{requestId}")]
        public async Task<IActionResult> GetNearbyProviders(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
                return BadRequest(new { status = "error", message = "Invalid requestId" });

            var result = await _emergencyService.GetNearbyProvidersAsync(requestId);

            if (result == null || !result.Any())
                return NotFound(new { status = "error", message = "No nearby providers found" });

            return Ok(new { status = "success", data = result });
        }

        [HttpPost("accept-offer")]
        [Authorize]
        public async Task<IActionResult> AcceptEmergencyOffer([FromBody] EmergencyOfferDTO dto)
        {
            var userId = GetUserIdFromClaims();

            if (dto == null || string.IsNullOrEmpty(dto.ProviderId) || string.IsNullOrEmpty(dto.RequestId))
                return BadRequest(new { status = "error", message = "Invalid offer data." });

            var result = await _emergencyService.AcceptEmergencyOfferAsync(dto.ProviderId, dto.RequestId);

            if (!result)
                return NotFound(new { status = "error", message = "Emergency request not found or already accepted." });

            return Ok(new { status = "success", message = "Emergency offer accepted successfully." });
        }

        [HttpPost("reject-offer")]
        [Authorize]
        public async Task<IActionResult> RejectEmergencyOffer([FromBody] EmergencyOfferDTO dto)
        {
            var userId = GetUserIdFromClaims();

            if (dto == null || string.IsNullOrEmpty(dto.ProviderId) || string.IsNullOrEmpty(dto.RequestId))
                return BadRequest(new { status = "error", message = "Invalid offer data." });

            var result = await _emergencyService.RejectEmergencyOfferAsync(dto.ProviderId, dto.RequestId);

            if (!result)
                return NotFound(new { status = "error", message = "Emergency request not found or already accepted." });

            return Ok(new { status = "success", message = "Emergency offer rejected." });
        }

        [HttpGet("all-emergency-requests")]
        public async Task<IActionResult> GetAllEmergencyRequests()
        {
            var requests = await _emergencyService.GetAllEmergencyRequestsAsync();
            if (requests == null || !requests.Any())
                return NotFound(new { status = "error", message = "No emergency requests found." });

            return Ok(new { status = "success", data = requests });
        }

        [HttpGet("emergency-request/{requestId}")]
        public async Task<IActionResult> GetEmergencyRequestById(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
                return BadRequest(new { status = "error", message = "Request ID is required." });

            var request = await _emergencyService.GetEmergencyRequestByIdAsync(requestId);
            if (request == null)
                return NotFound(new { status = "error", message = "Emergency request not found." });

            return Ok(new { status = "success", data = request });
        }
    }
}
