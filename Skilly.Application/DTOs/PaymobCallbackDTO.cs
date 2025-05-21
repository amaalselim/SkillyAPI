using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class PaymobCallbackDTO
    {
        public bool Success{ get; set; }
        public string OrderId { get; set; }
        public string Url { get; set; }
    }
}
