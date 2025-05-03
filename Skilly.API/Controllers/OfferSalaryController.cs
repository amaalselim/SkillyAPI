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
        public async Task<IActionResult> GetAlloffers()
        {
            try
            {
                var offer= await _unitOfWork._OfferSalaryRepository.GetAllOffersAsync();

                if (offer == null || !offer.Any())
                {
                    return NotFound(new { message = "No Offerrs found." });
                }
                return Ok(new { offer });
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
        [HttpGet("getAllOffersBy/{serviceId}")]
        public async Task<IActionResult> GetAlloffersByserviceId(string serviceId)
        {
            try
            {
                var offer = await _unitOfWork._OfferSalaryRepository.GetAllOffersByServiceId(serviceId);

                if (offer == null || !offer.Any())
                {
                    return NotFound(new { message = "No Offerrs found." });
                }
                return Ok(new { offer });
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

        [HttpGet("GetOfferBy/{Id}")]
        public async Task<IActionResult> GetOfferById([FromRoute] string Id)
        {
            try
            {
                var offer = await _unitOfWork._OfferSalaryRepository.GetOfferByIdAsync(Id);

                return Ok(new { offer });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("GetOfferBy/{serviceId}")]
        public async Task<IActionResult> GetOfferByserviceId([FromRoute] string serviceId)
        {
            try
            {
                var offer = await _unitOfWork._OfferSalaryRepository.GetOfferByserviceIdAsync(serviceId);   

                return Ok(new { offer });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpGet("GetOffersCountBy/{serviceId}")]
        public async Task<IActionResult> GetOffersCountByServiceId([FromRoute] string serviceId)
        {
            try
            {
                var offers = await _unitOfWork._OfferSalaryRepository.GetOffersCountByServiceIdAsync(serviceId);

                return Ok(new { offers });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpPost("AddOffer")]
        public async Task<IActionResult> AddOffer([FromBody]createofferDTO offersalaryDTO )
        {
            if (offersalaryDTO == null)
            {
                return BadRequest(new { message = "Invalid Offer data." });
            }

            try
            {
                var userId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authorized.");
                }
                await _unitOfWork._OfferSalaryRepository.AddOfferAsync(offersalaryDTO,userId);

                return Ok(new { message = "Offers added successfully.", data = offersalaryDTO });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpPut("EditOffersBy/{offerId}")]
        public async Task<IActionResult> EditOffer([FromBody] offersalaryDTO offersalaryDTO, [FromRoute] string offerId)
        {
            if (offersalaryDTO == null)
            {
                return BadRequest(new { message = "Invalid Offer data." });
            }

            try
            {
                await _unitOfWork._OfferSalaryRepository.UpdateOfferAsync(offersalaryDTO,offerId);

                return Ok(new { message = "Category updated successfully.", data = offersalaryDTO });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpDelete("DeleteOfferBy/{offerId}")]
        public async Task<IActionResult> DeleteOffer([FromRoute] string offerId)
        {
            try
            {
                await _unitOfWork._OfferSalaryRepository.DeleteOfferAsync(offerId);

                return Ok(new { message = "Offer deleted successfully." });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
