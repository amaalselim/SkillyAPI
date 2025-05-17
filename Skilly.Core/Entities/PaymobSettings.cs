using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Core.Entities
{
    public class PaymobSettings
    {
        public string ApiKey { get; set; }
        public string IframeId { get; set; }
        public string CardIntegrationId { get; set; }
        public string WalletIntegrationId { get; set; }
        public string PaypalIntegrationId { get; set; }
        public string CashIntegrationId { get; set; }
        public string HmacSecret { get; set; }
    }
}
