using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.Implementation;
using System.Threading.Tasks;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public BannerController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetAllBanners")]
        public async Task<IActionResult> GetAllBanners()
        {
            try
            {
                var banners = await _unitOfWork._BannerService.GetAllBannerAsync();

                if (banners == null || !banners.Any())
                {
                    return NotFound(new { message = "No banners found." });
                }
                return Ok(new { banners });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("UploadBanner")]
        public async Task<IActionResult> Upload([FromForm] BannerCreateDTO dto)
        {
            try
            {
                var banner = await _unitOfWork._BannerService.UploadBannerAsync(dto);
                return StatusCode(201, banner); // Created status code
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteBannerBy/{bannerId}")]
        public async Task<IActionResult> DeleteBanner([FromRoute] int bannerId)
        {
            try
            {
                await _unitOfWork._BannerService.DeleteBannerAsync(bannerId);

                return StatusCode(204);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
