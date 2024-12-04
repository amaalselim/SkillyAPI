using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Skilly.Application.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError($"ArgumentNullException occurred: {ex.Message}");
                await HandleExceptionAsync(httpContext, "Missing argument(s) in the request.", StatusCodes.Status400BadRequest);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"InvalidOperationException occurred: {ex.Message}");
                await HandleExceptionAsync(httpContext, "The operation could not be completed due to invalid data.", StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unexpected error occurred: {ex.Message}");
                await HandleExceptionAsync(httpContext, "An unexpected error occurred. Please try again later.", StatusCodes.Status500InternalServerError);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, string message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new { Success = false, Message = message };
            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}
