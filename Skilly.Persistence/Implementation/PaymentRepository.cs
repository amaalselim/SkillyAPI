using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Data;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using Skilly.Core.Enums;
using Skilly.Infrastructure.Implementation;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Skilly.Persistence.Implementation
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly PaymobService _paymobService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FirebaseV1Service _firebase;

        public PaymentRepository(ApplicationDbContext context,PaymobService paymobService, IHttpContextAccessor httpContextAccessor,FirebaseV1Service firebase)
        {
            _context = context;
            _paymobService = paymobService;
            _httpContextAccessor = httpContextAccessor;
            _firebase = firebase;
        }

        public async Task<string> HandlePaymentCallbackAsync(string id, bool success)
        {
            var payment = await _context.payments.FirstOrDefaultAsync(p => p.PaymobOrderId == id);
            if (payment == null)
                return "Payment not found";
            if (!success)
            {
                payment.PaymentStatus = "Failed";
                return "Failed";
            }
            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == payment.UserId);
            var userprofile = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == payment.UserId);

            payment.PaymentStatus = "paid";
            user.Points += 20;
            userprofile.useDiscount = false;

            var providerService = await _context.providerServices.FirstOrDefaultAsync(p => p.Id == payment.ProviderServiceId);

            if (providerService != null)
            {
                providerService.ServiceStatus = ServiceStatus.Paid;
                string title = "تم شراء الخدمة";
                string body = $"تم شراء الخدمة {providerService.Name} من قبل المستخدم {user.FirstName} {user.LastName}، برجاء البدء في تنفيذ الخدمة.";

                // إذا حابب تستخدم إشعارات firebase، شغل الكود تحت
                /*
                if (providerService?.UserProfile != null)
                {
                    await _firebase.SendNotificationAsync(
                        providerService.serviceProviderId,
                        title,
                        body
                    );

                    _context.notifications.Add(new Notifications
                    {
                        UserId = user.UserId,
                        Title = title,
                        Body = body,
                        userImg = user.Img,
                        CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                    });
                }
                */
            }
            else
            {
                var service = await _context.requestServices.FirstOrDefaultAsync(s => s.Id == payment.RequestServiceId);
                if (service != null)
                {
                    service.ServiceStatus = ServiceStatus.Paid;
                    string title = "تم شراء الخدمة";
                    string body = $"تم شراء الخدمة {service.Name} من قبل المستخدم {user.FirstName} {user.LastName}، برجاء البدء في تنفيذ الخدمة.";

                    //if (service?.UserProfile != null)
                    //{
                    //    await _firebase.SendNotificationAsync(
                    //        service.providerId,
                    //        title,
                    //        body
                    //    );

                    //    _context.notifications.Add(new Notifications
                    //    {
                    //        UserId = user.UserId,
                    //        Title = title,
                    //        Body = body,
                    //        userImg = user.Img,
                    //        CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                    //    });
                    //}
                }
            }

            await _context.SaveChangesAsync();

            return "Success";
        }


        public async Task<object> StartPaymentAsync(string serviceId)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not authenticated");

            var user = await _context.Users.FindAsync(userId);
            var userprofile=await _context.userProfiles.FirstOrDefaultAsync(u=>u.UserId== userId);

            if (user == null)
                throw new Exception("User not found");

            var providerService = await _context.providerServices.FirstOrDefaultAsync(p => p.Id == serviceId);

            decimal amount = 0;
            string relatedId = "";
            string serviceType = "";
            string providerId = "";

            if (providerService != null)
            {
                if (providerService.PriceDiscount != null && userprofile.useDiscount==true)
                {
                    amount = (decimal)providerService.PriceDiscount;
                }
                else
                {
                    amount = providerService.Price;
                }
                providerService.userprofileId = userId;
                relatedId = providerService.Id;
                serviceType = "provider";
                providerId = providerService.serviceProviderId;
            }
            else
            {
                var service = await _context.requestServices.FirstOrDefaultAsync(s => s.Id == serviceId);
                if (service != null)
                {
                    amount = service.Price;
                    relatedId = service.Id;
                    serviceType = "Request";
                    providerId = service.providerId;
                }
                else
                {
                    throw new Exception("Service not found");
                }
            }

            var authToken = await _paymobService.GetAuthTokenAsync();
            var orderId = await _paymobService.CreateOrderAsync(authToken, amount);
            var paymentToken = await _paymobService.CreatePaymentKeyAsync(authToken, orderId, amount,userprofile);

            var payment = new Payment
            {
                Amount = amount,
                PaymentStatus = "Pending",
                PaymentMethod = "Card",
                PaymobOrderId = orderId.ToString(),
                ProviderServiceId = (serviceType == "provider") ? relatedId : null,
                RequestServiceId = (serviceType == "Request") ? relatedId : null,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.payments.Add(payment);
            await _context.SaveChangesAsync();

            var iframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_paymobService.IframeId}?payment_token={paymentToken}";

            return new
            {
                iframeUrl
            };
        }


    }
}
