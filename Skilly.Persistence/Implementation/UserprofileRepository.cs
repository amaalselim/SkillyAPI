using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs.User;
using Skilly.Application.Exceptions;
using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Implementation
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public UserProfileRepository(ApplicationDbContext context,IMapper mapper,IImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task AddUserProfileAsync(UserProfileDTO UserProfileDTO, string userId)
        {
            var user= await _context.users.FirstOrDefaultAsync(u=>u.Id==userId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found.");
            }
            var existingProfile = await _context.userProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existingProfile != null)
            {
                throw new InvalidOperationException("User profile already exists.");
            }
            var UserProfile= _mapper.Map<UserProfile>(UserProfileDTO);   
            UserProfile.UserId=userId;
            UserProfile.FirstName=user.FirstName;
            UserProfile.LastName=user.LastName;
            UserProfile.Email = user.Email;
            UserProfile.PhoneNumber=user.PhoneNumber;
            if (UserProfileDTO.Img != null)
            {
                var path = @"Images/UserProfile/";
                UserProfile.Img =await _imageService.SaveFileAsync(UserProfileDTO.Img, path);
            }
            await _context.userProfiles.AddAsync(UserProfile);
            await _context.SaveChangesAsync();
        }
        public async Task EditUserProfileAsync(edituserProfileDTO UserProfileDTO, string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new UserNotFoundException("User not found.");
            }

            var UserProfile = await _context.userProfiles.FirstOrDefaultAsync(up => up.UserId == userId);
            if (UserProfile == null)
            {
                throw new UserProfileNotFoundException("User Profile not found.");
            }

            //_mapper.Map(UserProfileDTO, UserProfile);
            UserProfile.City = UserProfileDTO.City;
            UserProfile.StreetName = UserProfileDTO.StreetName;
            UserProfile.Governorate = UserProfileDTO.Governorate;
            UserProfile.Gender= UserProfileDTO.Gender;
            UserProfile.FirstName = UserProfileDTO.FirstName;
            UserProfile.LastName = UserProfileDTO.LastName;
            UserProfile.Email = user.Email;
            UserProfile.PhoneNumber = user.PhoneNumber;

            if (UserProfileDTO.Img != null)
            {
                var path = @"Images/UserProfile/";
                UserProfile.Img = await _imageService.SaveFileAsync(UserProfileDTO.Img, path);
            }

            _context.userProfiles.Update(UserProfile);
            await _context.SaveChangesAsync();
        }

        public async Task<UserProfile> GetByIdAsync(string userId)
        {
            return await _context.userProfiles.FirstOrDefaultAsync(up => up.UserId == userId);
        }

        public async Task<IEnumerable<UserProfile>> GetAllUserProfileAsync()
        {
            var users = await _context.userProfiles
                .ToListAsync();

            if (users == null || !users.Any())
            {
                return new List<UserProfile>();
            }
            return users;
        }

        public async Task DeleteUserProfileAsync(string userId)
        {
            
            var UserProfile = await _context.userProfiles.FirstOrDefaultAsync(up => up.UserId == userId);
            if (UserProfile == null)
            {
                throw new UserProfileNotFoundException("User profile not found.");
            }
            _context.userProfiles.Remove(UserProfile);
            await _context.SaveChangesAsync();
        }

        
    }
}
