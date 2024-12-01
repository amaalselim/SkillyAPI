using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Infrastructure.Abstract
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string userName, string Subject, string message);
    }
}
