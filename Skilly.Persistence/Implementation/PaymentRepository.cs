using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Skilly.Application.Abstract;
using Skilly.Application.DTOs;
using Skilly.Application.DTOs.Payment;
using Skilly.Core.Entities;
using Skilly.Core.Enums;
using Skilly.Infrastructure.Implementation;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using Skilly.Persistence.Migrations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vonage.Accounts;
using Vonage.SubAccounts;

namespace Skilly.Persistence.Implementation
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly PaymobService _paymobService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FirebaseV1Service _firebase;
        private readonly IChatService _chat;
        private readonly IMapper _mapper;

        public PaymentRepository(ApplicationDbContext context,PaymobService paymobService, IHttpContextAccessor httpContextAccessor,FirebaseV1Service firebase,IChatService chat,IMapper mapper)
        {
            _context = context;
            _paymobService = paymobService;
            _httpContextAccessor = httpContextAccessor;
            _firebase = firebase;
            _chat = chat;
            _mapper = mapper;
        }

        public async Task<(string result, string? providerId, string? chatId)> HandlePaymentCallbackAsync(string id, bool success)
        {
            var payment = await _context.payments.FirstOrDefaultAsync(p => p.PaymobOrderId == id);
            if (payment == null)
                return ("Payment not found", null, null);

            if (!success)
            {
                payment.PaymentStatus = "Failed";
                return ("Failed", null, null);
            }

            var user = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == payment.UserId);
            var userprofile = user;

            var providerService = await _context.providerServices.FirstOrDefaultAsync(p => p.Id == payment.ProviderServiceId);
            if (providerService != null)
            {
                var provider = await _context.users.FirstOrDefaultAsync(p => p.Id == providerService.uId);
                providerService.ServiceStatus = ServiceStatus.Paid;

                string title = "تم شراء الخدمة";
                decimal discountPercentage = 0.10m;
                decimal totalAmount = payment.Amount;
                decimal systemShare = totalAmount * discountPercentage;
                decimal providerAmount = totalAmount - systemShare;

                string body = $"تم شراء الخدمة {providerService.Name} من قبل المستخدم {user.FirstName} {user.LastName}، برجاء البدء في تنفيذ الخدمة. تم خصم {discountPercentage * 100}% كنسبة للسيستم، واستلمت مبلغ قدره {providerAmount} جنيه.";

                if (providerService?.Id != null)
                {
                    await _firebase.SendNotificationAsync(provider.FcmToken, title, body);
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

                userprofile.useDiscount = false;

                var imageUrls = await _context.providerServicesImages
                .Where(img => img.serviceId == providerService.Id)
                .Select(img => img.Img)
                .ToListAsync();

                var requestImages = imageUrls.Select(url => new requestServiceImage
                {
                    Img = url
                }).ToList();

                var request = new RequestService
                {
                    Name = providerService.Name,
                    Price = payment.Amount,
                    Deliverytime = providerService.Deliverytime,
                    startDate = DateOnly.FromDateTime(DateTime.Now),
                    categoryId = providerService.categoryId,
                    Notes = providerService.Notes,
                    userId = userprofile.Id,
                    uId = payment.UserId,
                    userImg = user.Img,
                    ServiceStatus = ServiceStatus.Paid,
                    providerId = providerService.uId,
                    Images = requestImages,
                    video = providerService.video
                };
                _context.requestServices.Add(request);

                var chat = await _context.chats.FirstOrDefaultAsync(c =>
                    (c.FirstUserId == user.UserId || c.FirstUserId == providerService.uId) &&
                    (c.SecondUserId == user.UserId || c.SecondUserId == providerService.uId));
                if (chat == null)
                {
                    var createChatDto = new CreateChatDTO
                    {
                        FirstUserId = user.UserId,
                        SecondUserId = providerService.uId
                    };
                    var createdChat = await _chat.CreateChatAsync(createChatDto);
                    chat = _mapper.Map<Chat>(createdChat);
                }

                payment.PaymentStatus = "paid";
                user.Points += 20;
                await _context.SaveChangesAsync();

                return ("Success", providerService.uId, chat.Id);
            }
            else
            {
                var service = await _context.requestServices.FirstOrDefaultAsync(s => s.Id == payment.RequestServiceId);
                if (service != null)
                {
                    var userrr = await _context.users.FirstOrDefaultAsync(s => s.Id == service.providerId);
                    service.ServiceStatus = ServiceStatus.Paid;

                    string title = "تم شراء الخدمة";
                    decimal discountPercentage = 0.10m;
                    decimal totalAmount = payment.Amount;
                    decimal systemShare = totalAmount * discountPercentage;
                    decimal providerAmount = totalAmount - systemShare;

                    string body = $"تم شراء الخدمة {service.Name} من قبل المستخدم {user.FirstName} {user.LastName}، برجاء البدء في تنفيذ الخدمة. تم خصم {discountPercentage * 100}% كنسبة للسيستم، واستلمت مبلغ قدره {providerAmount} جنيه.";

                    if (service?.Id != null)
                    {
                        await _firebase.SendNotificationAsync(userrr.FcmToken, title, body);
                        _context.notifications.Add(new Notifications
                        {
                            UserId = userrr.Id,
                            Title = title,
                            Body = body,
                            userImg = user.Img,
                            serviceId = service.Id,
                            CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                        });
                    }

                    var chat = await _context.chats.FirstOrDefaultAsync(c =>
                        (c.FirstUserId == user.UserId || c.FirstUserId == service.providerId) &&
                        (c.SecondUserId == user.UserId || c.SecondUserId == service.providerId));
                    if (chat == null)
                    {
                        var createChatDto = new CreateChatDTO
                        {
                            FirstUserId = user.UserId,
                            SecondUserId = service.uId
                        };
                        var createdChat = await _chat.CreateChatAsync(createChatDto);
                        chat = _mapper.Map<Chat>(createdChat);
                    }

                    payment.PaymentStatus = "paid";
                    user.Points += 20;
                    await _context.SaveChangesAsync();

                    return ("Success", service.providerId, chat.Id);
                }
                else
                {
                    var emergencyRequest = await _context.emergencyRequests.FirstOrDefaultAsync(s => s.Id == payment.EmergencyRequestId);
                    if (emergencyRequest != null)
                    {
                        var userr = await _context.users.FirstOrDefaultAsync(s => s.Id == emergencyRequest.UserId);
                        string title = "تم شراء الخدمة";
                        decimal discountPercentage = 0.10m;
                        decimal totalAmount = payment.Amount;
                        decimal systemShare = totalAmount * discountPercentage;
                        decimal providerAmount = totalAmount - systemShare;

                        string body = $"تم دفع خدمة الطوارئ التي تنص على  {emergencyRequest.ProblemDescription} من قبل المستخدم {user.FirstName} {user.LastName}، برجاء البدء في تنفيذ الخدمة. تم خصم {discountPercentage * 100}% كنسبة للسيستم، واستلمت مبلغ قدره {providerAmount} جنيه.";

                        var userrequest = await _context.serviceProviders
                            .Include(p => p.User)
                            .FirstOrDefaultAsync(u => u.UserId == emergencyRequest.AssignedProviderId);

                        if (emergencyRequest?.Id != null)
                        {
                            await _firebase.SendNotificationAsync(userrequest.User.FcmToken, title, body);
                            _context.notifications.Add(new Notifications
                            {
                                UserId = userrequest.UserId,
                                Title = title,
                                Body = body,
                                userImg = user.Img,
                                serviceId = emergencyRequest.Id,
                                CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                            });
                        }

                        emergencyRequest.Status = "paid";
                        emergencyRequest.Finalprice = 0;
                        var providerId = emergencyRequest.AssignedProviderId;
                        emergencyRequest.AssignedProviderId = null;


                        var emergencyAsRequest = new RequestService
                        {
                            Name = "طلب طارئ - " + emergencyRequest.ProblemDescription,
                            Price = payment.Amount,
                            Deliverytime = "فوري",
                            startDate = DateOnly.FromDateTime(DateTime.Now),
                            categoryId = emergencyRequest.CategoryId,
                            Notes = emergencyRequest.ProblemDescription,
                            userId = userprofile.Id,
                            uId = payment.UserId,
                            userImg = user.Img,
                            ServiceStatus = ServiceStatus.Paid,
                            providerId = providerId
                        };
                        _context.requestServices.Add(emergencyAsRequest);

                        var chat = await _context.chats.FirstOrDefaultAsync(c =>
                            (c.FirstUserId == user.UserId || c.FirstUserId == providerId) &&
                            (c.SecondUserId == user.UserId || c.SecondUserId == providerId));
                        if (chat == null)
                        {
                            var createChatDto = new CreateChatDTO
                            {
                                FirstUserId = user.UserId,
                                SecondUserId = providerId
                            };
                            var createdChat = await _chat.CreateChatAsync(createChatDto);
                            chat = _mapper.Map<Chat>(createdChat);
                        }

                        payment.PaymentStatus = "paid";
                        user.Points += 20;
                        await _context.SaveChangesAsync();

                        return ("Success", providerId, chat.Id);
                    }
                }
            }

            payment.PaymentStatus = "paid";
            user.Points += 20;
            await _context.SaveChangesAsync();
            return ("Success", null, null);
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
                var offers = await _context.offerSalaries.FirstOrDefaultAsync(o => o.serviceId == providerService.Id && o.Status == OfferStatus.Accepted);
                if (providerService.PriceDiscount != null && userprofile.useDiscount==true)
                {
                    amount = (decimal)providerService.PriceDiscount;
                }else if(offers != null)
                {
                    amount = offers.Salary;
                }
                else
                {
                    amount = providerService.Price;
                }
                providerService.userprofileId = userId;
                relatedId = providerService.Id;
                serviceType = "provider";
                providerId = providerService.uId;
            }
            else
            {
                var service = await _context.requestServices.FirstOrDefaultAsync(s => s.Id == serviceId);
                if (service != null)
                {
                    var offers = await _context.offerSalaries.FirstOrDefaultAsync(o => o.requestserviceId == service.Id && o.Status == OfferStatus.Accepted);
                    if (offers != null)
                    {
                        amount = offers.Salary;
                    }
                    else
                    {
                        amount = service.Price;
                    }
                        
                    relatedId = service.Id;
                    serviceType = "Request";
                    providerId = service.providerId;
                }
                else
                {
                    var request = await _context.emergencyRequests.FirstOrDefaultAsync(r => r.Id == serviceId && r.UserId==userId);
                    if (request != null)
                    {
                        amount = request.Finalprice??0;
                        relatedId = request.Id;
                        serviceType = "Emergency";
                        providerId = request.AssignedProviderId;
                    }
                    else
                    {
                        throw new Exception("Service not found");
                    }
                }
            }

            var authToken = await _paymobService.GetAuthTokenAsync();
            var orderId = await _paymobService.CreateOrderAsync(authToken, amount);
            var paymentToken = await _paymobService.CreatePaymentKeyAsync2(authToken, orderId, amount,userprofile);

            var payment = new Payment
            {
                Amount = amount,
                PaymentStatus = "Pending",
                PaymentMethod = "Card",
                PaymobOrderId = orderId.ToString(),
                ProviderServiceId = (serviceType == "provider") ? relatedId : null,
                RequestServiceId = (serviceType == "Request") ? relatedId : null,
                EmergencyRequestId = (serviceType == "Emergency") ? relatedId : null,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                ProviderId = providerId
            };

            _context.payments.Add(payment);
            await _context.SaveChangesAsync();
           
            var wallet = await _context.wallets.FirstOrDefaultAsync(w => w.ProviderId == providerId);
            if (wallet != null)
            {
                wallet.IsTransmitted = false;
            }


            var iframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_paymobService.IframeId}?payment_token={paymentToken}";

            return new
            {
                iframeUrl
            };
        }



        public async Task<object> StartPaymentAsync(string serviceId, string redirectUrl)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not authenticated");

            var user = await _context.Users.FindAsync(userId);
            var userprofile = await _context.userProfiles.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new Exception("User not found");

            var providerService = await _context.providerServices.FirstOrDefaultAsync(p => p.Id == serviceId);

            decimal amount = 0;
            string relatedId = "";
            string serviceType = "";
            string providerId = "";

            if (providerService != null)
            {
                var offers = await _context.offerSalaries.FirstOrDefaultAsync(o => o.serviceId == providerService.Id && o.Status == OfferStatus.Accepted);
                if (providerService.PriceDiscount != null && userprofile.useDiscount == true)
                {
                    amount = (decimal)providerService.PriceDiscount;
                }
                else if (offers != null)
                {
                    amount = offers.Salary;
                }
                else
                {
                    amount = providerService.Price;
                }
                providerService.userprofileId = userId;
                relatedId = providerService.Id;
                serviceType = "provider";
                providerId = providerService.uId;
            }
            else
            {
                var service = await _context.requestServices.FirstOrDefaultAsync(s => s.Id == serviceId);
                if (service != null)
                {
                    var offers = await _context.offerSalaries.FirstOrDefaultAsync(o => o.requestserviceId == service.Id && o.Status == OfferStatus.Accepted);
                    amount = offers?.Salary ?? service.Price;

                    relatedId = service.Id;
                    serviceType = "Request";
                    providerId = service.providerId;
                }
                else
                {
                    var request = await _context.emergencyRequests.FirstOrDefaultAsync(r => r.Id == serviceId && r.UserId == userId);
                    if (request != null)
                    {
                        amount = request.Finalprice ?? 0;
                        relatedId = request.Id;
                        serviceType = "Emergency";
                        providerId = request.AssignedProviderId;
                    }
                    else
                    {
                        throw new Exception("Service not found");
                    }
                }
            }

            var authToken = await _paymobService.GetAuthTokenAsync();
            var orderId = await _paymobService.CreateOrderAsync(authToken, amount);
            var paymentToken = await _paymobService.CreatePaymentKeyAsync(authToken, orderId, amount, userprofile, redirectUrl);

            var payment = new Payment
            {
                Amount = amount,
                PaymentStatus = "Pending",
                PaymentMethod = "Card",
                PaymobOrderId = orderId.ToString(),
                ProviderServiceId = (serviceType == "provider") ? relatedId : null,
                RequestServiceId = (serviceType == "Request") ? relatedId : null,
                EmergencyRequestId = (serviceType == "Emergency") ? relatedId : null,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                ProviderId=providerId
            };

            _context.payments.Add(payment);
            var wallet = await _context.wallets.FirstOrDefaultAsync(w => w.ProviderId == providerId);
            if (wallet != null)
            {
                wallet.IsTransmitted = false;
            }
            await _context.SaveChangesAsync();

            var iframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_paymobService.IframeId}?payment_token={paymentToken}";


            return new
            {
                iframeUrl
            };
        }

        public async Task<IEnumerable<Payment>> GetAllTransactions()
        {
            var trans = await _context.payments
                .Include(p=>p.User)
                .ToListAsync();

            if (trans == null || !trans.Any())
            {
                return new List<Payment>();
            }

            var transDtos = trans.Select(item => new Payment
            {
                Id = item.Id,
                Amount = item.Amount,
                PaymentStatus = item.PaymentStatus,
                PaymentMethod = item.PaymentMethod,
                CreatedAt = item.CreatedAt,
                ProviderServiceId = item.ProviderServiceId,
                RequestServiceId = item.RequestServiceId,
                EmergencyRequestId = item.EmergencyRequestId,
                PaymobOrderId = item.PaymobOrderId,
                TransactionId = item.TransactionId,
                UserId = item.UserId,
            }).ToList();

            return trans;
        }

        //Wallet
        public async Task<Wallet> ProcessPaymentAsync(string providerId)
        {
            var payments = await _context.payments
                .Where(p => p.ProviderId == providerId && p.PaymentStatus == "paid" && !p.IsProcessed)
                .ToListAsync();

            decimal totalProviderAmount = payments.Sum(p => p.Amount * 0.8m);

            var providerWallet = await _context.wallets
                .Include(w => w.provider)
                .FirstOrDefaultAsync(w => w.ProviderId == providerId);

            if (providerWallet == null)
            {
                providerWallet = new Wallet
                {
                    ProviderId = providerId,
                    Balance = 0
                };
                _context.wallets.Add(providerWallet);
            }

            providerWallet.Balance += totalProviderAmount;

            foreach (var payment in payments)
            {
                payment.IsProcessed = true;
            }
            await _context.SaveChangesAsync();
            string providerName = providerWallet.provider != null
               ? providerWallet.provider.FirstName + " " + providerWallet.provider.LastName
               : "Unknown Provider";
            return new Wallet
            {
                ProviderId = providerId,
                ProviderName=providerName,
                Balance = !providerWallet.IsTransmitted ? providerWallet.Balance :totalProviderAmount
            };
        }


        public async Task<List<GroupedTransactionsDTO>> GetTransactionsGroupedByDate(string providerId)
        {
            var transactions = await _context.payments
                .Include(p => p.User)
                .Include(p => p.ProviderService)
                .Include(p => p.RequestService)
                .Include(p => p.EmergencyRequest)
                .Where(p => p.ProviderId == providerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var culture = new CultureInfo("ar-EG");
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            var transactionDtos = transactions.Select(trans => new TransactionDTO
            {
                Id = trans.Id,
                UserId = trans.UserId,
                CreatedAt = trans.CreatedAt,
                UserName = trans.User?.FirstName + " " + trans.User?.LastName,
                Amount = trans.Amount,
                Message = $"لقد استلمت {trans.Amount} ج.م بنجاح مقابل خدمة " +
                    (trans.ProviderService != null ? trans.ProviderService.Name :
                     trans.RequestService != null ? trans.RequestService.Name :
                     trans.EmergencyRequest != null ? trans.EmergencyRequest.ProblemDescription :
                     "غير معروفة") + ".",
                FormattedCreatedAt = trans.CreatedAt
                    .ToString("dd MMMM yyyy - hh:mm tt", culture)
                    .Replace("AM", "صباحًا").Replace("PM", "مساءً")
            }).ToList();

            var grouped = new List<GroupedTransactionsDTO>
            {
                new GroupedTransactionsDTO
                {
                    Title = "اليوم",
                    Transactions = transactionDtos
                        .Where(t => t.CreatedAt.Date == today)
                        .ToList()
                },
                new GroupedTransactionsDTO
                {
                    Title = "أمس",
                    Transactions = transactionDtos
                        .Where(t => t.CreatedAt.Date == yesterday)
                        .ToList()
                },
                new GroupedTransactionsDTO
                {
                    Title = "منذ أيام",
                    Transactions = transactionDtos
                        .Where(t => t.CreatedAt.Date < yesterday)
                        .ToList()
                }
            };

            return grouped.Where(g => g.Transactions.Any()).ToList();
        }
        public string GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId;
        }
        public async Task<string> RequestWithdrawAsync(WithdrawRequestDTO request)
        {
            var providerId = GetUserId();
            request.ProviderId = providerId;
            var wallet = await _context.wallets
                .FirstOrDefaultAsync(w => w.ProviderId == request.ProviderId);
            request.Amount = wallet.Balance;

            if (wallet == null)
                throw new Exception("Wallet not found.");

            if (wallet.Balance < request.Amount)
                throw new Exception("Insufficient balance.");

            if (request.WithdrawMethod == "محفظه" && string.IsNullOrEmpty(request.PhoneNumber))
                throw new Exception("Phone number is required for wallet withdrawal.");

            if (request.WithdrawMethod == "INSTAPAY" && string.IsNullOrEmpty(request.InstapayEmail))
                throw new Exception("Instapay email is required.");

            var lastPayment = await _context.payments
                .Where(p => p.ProviderId == request.ProviderId && p.PaymentStatus == "paid")
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastPayment != null)
            {
                lastPayment.WithdrawMethod = request.WithdrawMethod;

                if (request.WithdrawMethod == "محفظه")
                    lastPayment.PhoneNumber = request.PhoneNumber;

                if (request.WithdrawMethod == "INSTAPAY")
                    lastPayment.InstapayEmail = request.InstapayEmail;
            }
            wallet.IsTransmitted = true;
            //wallet.Balance = 0;

            await _context.SaveChangesAsync();

            return "Withdrawal request submitted Successfully";
        }


    }
}
