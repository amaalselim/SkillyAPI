using Microsoft.AspNetCore.Identity;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.Abstract
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterDTO registerDTO);
        Task<object> LoginAsync(LoginDTO loginDTO);
        Task<IdentityResult> UpdatePasswordAsync(UpdatePasswordDTO updatePasswordDTO);
        Task SendResetPasswordEmailAsync(string email);
        Task<string> GeneratePasswordResetTokenAsync(ForgetPasswordDTO forgetPasswordDTO);
        Task<User> FindByEmailAsync(string email);
        Task<bool> VerifyEmailCodeAsync(VerficationCodeDTO verficationCodeDTO);
    }
}
