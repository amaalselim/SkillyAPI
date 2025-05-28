using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs.Emergency;
using Skilly.Core.Entities;
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
            {
                throw new UnauthorizedAccessException("User not authorized.");
            }
            return userId;
        }
        [HttpPost("create-emergency-request")]
        public async Task<IActionResult> CreateEmergencyRequest([FromBody] EmergencyRequestDTO emergencyRequestDTO)
        {
            var userId = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }
            if (emergencyRequestDTO == null)
            {
                return BadRequest("Emergency request data is required.");
            }
            try
            {
                await _emergencyService.CreateEmergencyRequestAsync(emergencyRequestDTO, userId);
                return Ok(new { message = "Emergency request created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }

        }

        [HttpPost("nearby-providers/{requestId}")]
        public async Task<IActionResult> GetNearbyProviders(string requestId)
        {
            if (requestId == null)
                return BadRequest("Invalid requestId");

            var result = await _emergencyService.GetNearbyProvidersAsync(requestId);

            if (result == null || !result.Any())
                return NotFound("No nearby providers found");

            return Ok(new { result });
        }
        [HttpPost("accept-offer")]
        public async Task<IActionResult> AcceptEmergencyOffer([FromBody] EmergencyOfferDTO emergencyOfferDTO)
        {
            var userId = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }
            if (emergencyOfferDTO == null || string.IsNullOrEmpty(emergencyOfferDTO.ProviderId) || string.IsNullOrEmpty(emergencyOfferDTO.RequestId))
            {
                return BadRequest("Invalid offer data.");
            }
            var result = await _emergencyService.AcceptEmergencyOfferAsync(emergencyOfferDTO.ProviderId, emergencyOfferDTO.RequestId);
            if (result)
            {
                return Ok(new { message = "Emergency offer accepted successfully." });
            }
            return NotFound("Emergency request not found or already accepted.");
        }
        [HttpPost("reject-offer")]
        public async Task<IActionResult> RejectEmergencyOffer([FromBody] EmergencyOfferDTO emergencyOfferDTO)
        {
            var userId = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }
            if (emergencyOfferDTO == null || string.IsNullOrEmpty(emergencyOfferDTO.ProviderId) || string.IsNullOrEmpty(emergencyOfferDTO.RequestId))
            {
                return BadRequest("Invalid offer data.");
            }
            var result = await _emergencyService.RejectEmergencyOfferAsync(emergencyOfferDTO.ProviderId, emergencyOfferDTO.RequestId);
            if (result)
            {
                return Ok(new { message = "Emergency offer rejected." });
            }
            return NotFound("Emergency request not found or already accepted.");
        }
        [HttpGet("all-emergency-requests")]
        public async Task<IActionResult> GetAllEmergencyRequests()
        {
            var requests = await _emergencyService.GetAllEmergencyRequestsAsync();
            if (requests == null || !requests.Any())
            {
                return NotFound("No emergency requests found.");
            }
            return Ok(new { requests });
        }
        [HttpGet("emergency-request/{requestId}")]
        public async Task<IActionResult> GetEmergencyRequestById(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                return BadRequest("Request ID is required.");
            }
            var request = await _emergencyService.GetEmergencyRequestByIdAsync(requestId);
            if (request == null)
            {
                return NotFound("Emergency request not found.");
            }
            return Ok(new { request });


        }
    }
}
