using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.Exceptions
{
    public class ServiceProviderNotFoundException : Exception
    {
        public ServiceProviderNotFoundException(string message) : base(message) { }
    }
}
