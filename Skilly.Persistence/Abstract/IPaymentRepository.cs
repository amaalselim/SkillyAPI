using Skilly.Application.DTOs.Payment;
using Skilly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Abstract
{
    public interface IPaymentRepository
    {
        Task<object> StartPaymentAsync(string serviceId);
        Task<object> StartPaymentAsync(string serviceId, string redirectUrl);
        Task<(string result, string? providerId,string? chatId)> HandlePaymentCallbackAsync(string id, bool success);
        Task<IEnumerable<Payment>> GetAllTransactions();
        Task<Wallet> ProcessPaymentAsync(string paymentId);
        Task<List<GroupedTransactionsDTO>> GetTransactionsGroupedByDate(string providerId);
        Task<string> RequestWithdrawAsync(WithdrawRequestDTO request);
    }
}
