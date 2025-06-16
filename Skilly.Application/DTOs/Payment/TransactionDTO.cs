using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.Payment
{
    public class TransactionDTO
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FormattedCreatedAt { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public decimal Amount { get; set; }
        public string Message { get; set; }
    }
}
