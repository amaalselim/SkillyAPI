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
using Vonage.SubAccounts;

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

            

            var providerService = await _context.providerServices.FirstOrDefaultAsync(p => p.Id == payment.ProviderServiceId);
            var provider = await _context.users.FirstOrDefaultAsync(p => p.Id == providerService.uId);
            if (providerService != null)
            {
                providerService.ServiceStatus = ServiceStatus.Paid;
                string title = "تم شراء الخدمة";
                decimal discountPercentage = 0.20m;
                decimal totalAmount=payment.Amount;
                decimal systemShare = totalAmount * discountPercentage;
                decimal providerAmount = totalAmount - systemShare;

                string body = $"تم شراء الخدمة {providerService.Name} من قبل المستخدم {user.FirstName} {user.LastName}، برجاء البدء في تنفيذ الخدمة. تم خصم {discountPercentage * 100}% كنسبة للسيستم، واستلمت مبلغ قدره {providerAmount} جنيه.";


                if (providerService?.Id != null)
                {
                    await _firebase.SendNotificationAsync(
                        provider.FcmToken,
                        title,
                        body
                    );

                    _context.notifications.Add(new Notifications
                    {
                        UserId = providerService.uId,
                        Title = title,
                        Body = body,
                        userImg = user.Img,
                        serviceId = providerService.Id,
                        CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                    });
                }
                
            }
            else
            {
                var service = await _context.requestServices.FirstOrDefaultAsync(s => s.Id == payment.RequestServiceId);
                var userr = await _context.users.FirstOrDefaultAsync(s => s.Id == service.userId);
                if (service != null)
                {
                    service.ServiceStatus = ServiceStatus.Paid;
                    string title = "تم شراء الخدمة";
                    decimal discountPercentage = 0.20m;
                    decimal totalAmount = payment.Amount;
                    decimal systemShare = totalAmount * discountPercentage;
                    decimal providerAmount = totalAmount - systemShare;

                    string body = $"تم شراء الخدمة {service.Name} من قبل المستخدم {user.FirstName} {user.LastName}، برجاء البدء في تنفيذ الخدمة. تم خصم {discountPercentage * 100}% كنسبة للسيستم، واستلمت مبلغ قدره {providerAmount} جنيه.";


                    if (service?.Id != null)
                    {
                        await _firebase.SendNotificationAsync(
                            userr.FcmToken,
                            title,
                            body
                        );

                        _context.notifications.Add(new Notifications
                        {
                            UserId = service.userId,
                            Title = title,
                            Body = body,
                            userImg = user.Img,
                            serviceId = service.Id,
                            CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                        });
                    }
                }
            }
            payment.PaymentStatus = "paid";
            user.Points += 20;
            userprofile.useDiscount = false;

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
