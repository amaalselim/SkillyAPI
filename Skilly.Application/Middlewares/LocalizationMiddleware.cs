using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Application.Middlewares
{
    public class LocalizationMiddleware
    {
        private readonly RequestDelegate _next;
        public LocalizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var culture = httpContext.Request.Headers["Accept-Language"].ToString();
            culture=string.IsNullOrEmpty(culture)? "ar-EG" : culture;

            var cultureInfo=new CultureInfo(culture);

            CultureInfo.DefaultThreadCurrentCulture=cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture=cultureInfo;
            
            await _next(httpContext);
        }
    }
}
