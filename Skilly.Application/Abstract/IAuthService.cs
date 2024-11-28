using Microsoft.AspNetCore.Identity;
using Skilly.Application.DTOs;
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
    }
}
