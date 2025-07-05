using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skilly.Application.DTOs;
using Skilly.Persistence.Abstract;
using System.Threading.Tasks;

namespace Skilly.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            var banners = await _unitOfWork._BannerService.GetAllBannerAsync();

            if (banners == null || !banners.Any())
                return NotFound(new { message = "No banners found." });

            return Ok(new { banners });
        }

        [HttpPost("UploadBanner")]
        public async Task<IActionResult> Upload([FromForm] BannerCreateDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid banner data." });

            var banner = await _unitOfWork._BannerService.UploadBannerAsync(dto);
            return CreatedAtAction(nameof(GetAllBanners), new { message = "Banner uploaded successfully.", data = banner });
        }

        [HttpDelete("DeleteBannerBy/{bannerId}")]
        public async Task<IActionResult> DeleteBanner(int bannerId)
        {

            await _unitOfWork._BannerService.DeleteBannerAsync(bannerId);
            return Ok(new { message = "Banner deleted successfully." });
        }
    }
}
