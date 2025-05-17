using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.DTOs;
using Skilly.Core.Entities;
using Skilly.Infrastructure.Implementation;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using System;
using System.Collections.Generic;
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

        public PaymentRepository(ApplicationDbContext context,PaymobService paymobService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _paymobService = paymobService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> HandlePaymentCallbackAsync(string id)
        {
            var payment = await _context.payments.FirstOrDefaultAsync(p=>p.PaymobOrderId == id);
            if (payment == null)
                payment.PaymentStatus = "failed";

            payment.PaymentStatus = "paid";
            await _context.SaveChangesAsync();
            return "Success";

        }

        public async Task<object> StartPaymentAsync(string serviceId)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not authenticated");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var providerService = await _context.providerServices.FirstOrDefaultAsync(p => p.Id == serviceId);

            decimal amount = 0;
            string relatedId = "";
            string serviceType = "";

            if (providerService != null)
            {
                amount = providerService.Price;
                relatedId = providerService.Id;
                serviceType = "provider";
            }
            else
            {
                var service = await _context.requestServices.FirstOrDefaultAsync(s => s.Id == serviceId);
                if (service != null)
                {
                    amount = service.Price;
                    relatedId = service.Id;
                    serviceType = "Request";
                }
                else
                {
                    throw new Exception("Service not found");
                }
            }

            var authToken = await _paymobService.GetAuthTokenAsync();
            var orderId = await _paymobService.CreateOrderAsync(authToken, amount);
            var paymentToken = await _paymobService.CreatePaymentKeyAsync(authToken, orderId, amount);

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
                iframeUrl,
                message = "Redirect to this URL to complete payment",
                user = new
                {
                    user.Id,
                    user.Email,
                    user.PhoneNumber
                }
            };
        }


    }
}
