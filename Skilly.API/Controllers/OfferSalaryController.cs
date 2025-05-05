using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class OfferSalaryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public OfferSalaryController(IUnitOfWork unitOfWork)
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

        [HttpGet("getAllOffers")]
        public async Task<IActionResult> GetAllOffers()
        {
            try
            {
                var offers = await _unitOfWork._OfferSalaryRepository.GetAllOffersAsync();

                if (offers == null || !offers.Any())
                {
                    return NotFound(new { message = "No offers found." });
                }

                return Ok(new { offers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("getAllOffersBy/{serviceId}")]
        public async Task<IActionResult> GetAllOffersByServiceId(string serviceId)
        {
            try
            {
                var offers = await _unitOfWork._OfferSalaryRepository.GetAllOffersByServiceId(serviceId);

                if (offers == null || !offers.Any())
                {
                    return NotFound(new { message = "No offers found for the specified service." });
                }

                return Ok(new { offers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("GetOfferBy/{Id}")]
        public async Task<IActionResult> GetOfferById([FromRoute] string Id)
        {
            try
            {
                var offer = await _unitOfWork._OfferSalaryRepository.GetOfferByIdAsync(Id);

                if (offer == null)
                {
                    return NotFound(new { message = "Offer not found." });
                }

                return Ok(new { offer });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("GetOfferBy/{serviceId}")]
        public async Task<IActionResult> GetOfferByServiceId([FromRoute] string serviceId)
        {
            try
            {
                var offer = await _unitOfWork._OfferSalaryRepository.GetOfferByIdAsync(serviceId);

                if (offer == null)
                {
                    return NotFound(new { message = "Offer not found for the specified service." });
                }

                return Ok(new { offer });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("GetOffersCountBy/{serviceId}")]
        public async Task<IActionResult> GetOffersCountByServiceId([FromRoute] string serviceId)
        {
            try
            {
                var offersCount = await _unitOfWork._OfferSalaryRepository.GetOffersCountByServiceIdAsync(serviceId);

                return Ok(new { offersCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("AddOffer")]
        public async Task<IActionResult> AddOffer([FromBody] createofferDTO offerSalaryDTO)
        {
            if (offerSalaryDTO == null)
            {
                return BadRequest(new { message = "Invalid offer data." });
            }

            try
            {
                var userId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authorized." });
                }

                await _unitOfWork._OfferSalaryRepository.AddOfferAsync(offerSalaryDTO, userId);

                return Ok(new { message = "Offer added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPut("EditOfferBy/{offerId}")]
        public async Task<IActionResult> EditOffer([FromBody] offersalaryDTO offerSalaryDTO, [FromRoute] string offerId)
        {
            if (offerSalaryDTO == null)
            {
                return BadRequest(new { message = "Invalid offer data." });
            }

            try
            {
                var offer = await _unitOfWork._OfferSalaryRepository.GetOfferByIdAsync(offerId);
                if (offer == null)
                {
                    return NotFound(new { message = "Offer not found." });
                }

                await _unitOfWork._OfferSalaryRepository.UpdateOfferAsync(offerSalaryDTO, offerId);

                return Ok(new { message = "Offer updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpDelete("DeleteOfferBy/{offerId}")]
        public async Task<IActionResult> DeleteOffer([FromRoute] string offerId)
        {
            try
            {
                var offer = await _unitOfWork._OfferSalaryRepository.GetOfferByIdAsync(offerId);
                if (offer == null)
                {
                    return NotFound(new { message = "Offer not found." });
                }

                await _unitOfWork._OfferSalaryRepository.DeleteOfferAsync(offerId);

                return Ok(new { message = "Offer deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
