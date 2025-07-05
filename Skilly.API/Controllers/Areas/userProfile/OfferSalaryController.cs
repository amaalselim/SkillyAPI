using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs;
using Skilly.Persistence.Abstract;
using System.Security.Claims;

namespace Skilly.API.Controllers.Areas.userProfile
{
    [Route("api/[controller]")]
    [ApiController]
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
                throw new UnauthorizedAccessException("User not authorized.");
            return userId;
        }

        [HttpGet("getAllOffers")]
        public async Task<IActionResult> GetAllOffers()
        {
            try
            {
                var offers = await _unitOfWork._OfferSalaryRepository.GetAllOffersAsync();
                return Ok(new { offers });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getAllOffersBy/{serviceId}")]
        public async Task<IActionResult> GetAllOffersByServiceId(string serviceId)
        {
            try
            {
                var offers = await _unitOfWork._OfferSalaryRepository.GetAllOffersByServiceId(serviceId);
                return Ok(new { offers });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetOfferBy/{Id}")]
        public async Task<IActionResult> GetOfferById(string Id)
        {
            try
            {
                var offer = await _unitOfWork._OfferSalaryRepository.GetOfferByIdAsync(Id);
                if (offer == null)
                    return NotFound(new { message = "Offer not found." });

                return Ok(new { offer });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("GetOffersCountBy/{serviceId}")]
        public async Task<IActionResult> GetOffersCountByServiceId(string serviceId)
        {
            try
            {
                var offersCount = await _unitOfWork._OfferSalaryRepository.GetOffersCountByServiceIdAsync(serviceId);
                return Ok(new { offersCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("AddOffer")]
        [Authorize]
        public async Task<IActionResult> AddOffer([FromBody] createofferDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid offer data." });

            try
            {
                var userId = GetUserIdFromClaims();
                await _unitOfWork._OfferSalaryRepository.AddOfferAsync(dto, userId);
                return Ok(new { message = "Offer added successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("EditOfferBy/{offerId}")]
        [Authorize]
        public async Task<IActionResult> EditOffer([FromBody] offersalaryDTO dto, string offerId)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid offer data." });

            try
            {
                var offer = await _unitOfWork._OfferSalaryRepository.GetOfferByIdAsync(offerId);
                if (offer == null)
                    return NotFound(new { message = "Offer not found." });

                await _unitOfWork._OfferSalaryRepository.UpdateOfferAsync(dto, offerId);
                return Ok(new { message = "Offer updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteOfferBy/{offerId}")]
        [Authorize]
        public async Task<IActionResult> DeleteOffer(string offerId)
        {
            try
            {
                var offer = await _unitOfWork._OfferSalaryRepository.GetOfferByIdAsync(offerId);
                if (offer == null)
                    return NotFound(new { message = "Offer not found." });

                await _unitOfWork._OfferSalaryRepository.DeleteOfferAsync(offerId);
                return Ok(new { message = "Offer deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("AcceptOffer/{id}")]
        [Authorize]
        public async Task<IActionResult> AcceptOffer(string id)
        {
            try
            {
                var result = await _unitOfWork._OfferSalaryRepository.AcceptOfferAsync(id);
                if (!result)
                    return NotFound(new { message = "Offer not found." });

                return Ok(new { message = "Offer accepted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("RejectOffer/{id}")]
        [Authorize]
        public async Task<IActionResult> RejectOffer(string id)
        {
            try
            {
                var result = await _unitOfWork._OfferSalaryRepository.RejectOfferAsync(id);
                if (!result)
                    return NotFound(new { message = "Offer not found." });

                return Ok(new { message = "Offer rejected successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
