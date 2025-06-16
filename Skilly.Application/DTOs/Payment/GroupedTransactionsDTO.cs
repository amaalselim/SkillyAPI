using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs.Payment
{
    public class GroupedTransactionsDTO
    {
        public string Title { get; set; } 
        public List<TransactionDTO> Transactions { get; set; }
    }
}
