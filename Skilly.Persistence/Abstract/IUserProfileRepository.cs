using Skilly.Application.DTOs.User;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IUserProfileRepository 
    {
        Task<IEnumerable<UserProfile>> GetAllUserProfileAsync();
        Task<UserProfile> GetByIdAsync(string id);
        Task AddUserProfileAsync(UserProfileDTO userProfileDTO, string userId);
        Task EditUserProfileAsync(edituserProfileDTO userProfileDTO, string userId);
        Task DeleteUserProfileAsync(string userId);
    }
}
