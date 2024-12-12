using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.Exceptions
{
    public class ProviderServiceNotFoundException : Exception
    {
        public ProviderServiceNotFoundException(string message) : base(message) { }
    }
}
