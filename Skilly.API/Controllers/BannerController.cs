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
                var banners= await _unitOfWork._BannerService.GetAllBannerAsync();

                if (banners == null || !banners.Any())
                {
                    return NotFound(new { message = "No banners found." });
                }
                return Ok(new { banners });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("UploadBanner")]
        public async Task<IActionResult> Upload([FromForm] BannerCreateDTO dto)
        {
            try
            {
                var banner = await _unitOfWork._BannerService.UploadBannerAsync(dto);
                return Ok(banner);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("DeleteBannerBy/{bannerId}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int bannerId)
        {
            try
            {
                await _unitOfWork._BannerService.DeleteBannerAsync(bannerId);

                return Ok(new { message = "banner deleted successfully." });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
