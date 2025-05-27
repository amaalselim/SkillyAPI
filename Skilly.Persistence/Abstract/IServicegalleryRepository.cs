using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IServicegalleryRepository 
    {  
        Task<IEnumerable<Servicesgallery>> GetAllServiceGallery();
        Task<IEnumerable<Servicesgallery>> GetAllgalleryByProviderId(string providerId);
        Task<IEnumerable<Servicesgallery>> GetAllgalleryByPProviderId(string providerId);
        Task<Servicesgallery> GetServiceGalleryByIdAsync(string galleryId, string userId);
        Task AddServiceGallery(servicegalleryDTO servicegalleryDTO, string userId);
        Task EditServiceGallery(servicegalleryDTO servicegalleryDTO, string userId, string galleryId);
        Task DeleteServiceGalleryAsync(string galleryId, string userId);
    }
}
