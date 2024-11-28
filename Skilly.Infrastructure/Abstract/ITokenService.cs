using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Infrastructure.Abstract
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(IEnumerable<Claim> claims, bool rememberMe);
    }
}
