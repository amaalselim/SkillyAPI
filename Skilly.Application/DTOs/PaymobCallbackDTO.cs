using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.DTOs
{
    public class PaymobCallbackDTO
    {
        public PaymobCallbackObject obj { get; set; }
        public PaymobCallbackOrder Order { get; set; }
    }
    public class PaymobCallbackObject
    {
        public bool success { get; set; }
    }
    public class PaymobCallbackOrder
    {
        public int Id { get; set; }
    }
}
