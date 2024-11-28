using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.Abstract
{
    public interface IClaimsService
    {
        Task<List<Claim>> GetClaimsAsync(string PhoneNumber, string userId);
    }
}
