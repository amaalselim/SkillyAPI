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
        Task<string> HandlePaymentCallbackAsync(string id, bool success);
        Task<IEnumerable<Payment>> GetAllTransactions();
    }
}
