using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IBannerService
    {
        Task<Banner> UploadBannerAsync(BannerCreateDTO bannerCreateDTO);
        Task<IEnumerable<Banner>> GetAllBannerAsync();
        Task DeleteBannerAsync(string id);
    }
}
