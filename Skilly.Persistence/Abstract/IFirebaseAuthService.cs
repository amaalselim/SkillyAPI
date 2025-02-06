using Microsoft.AspNetCore.Identity;
using Skilly.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.Abstract
{
    public interface IFirebaseAuthService
    {
        Task<FirebaseUserInfoDTO> VerifyGoogleTokenAsync (string idToken);
        //Task<bool> VerifyOtpAsync(string phoneNumber, int otpCode);


    }
}
