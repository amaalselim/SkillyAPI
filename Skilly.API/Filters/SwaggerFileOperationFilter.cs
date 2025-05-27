using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Skilly.API.Filters
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameters = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) ||
                            (p.ParameterType.IsGenericType &&
                             p.ParameterType.GetGenericArguments().First() == typeof(IFormFile)))
                .ToList();

            if (!fileParameters.Any())
                return;

            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = fileParameters.ToDictionary(
                            p => p.Name,
                            p => new OpenApiSchema { Type = "string", Format = "binary" }),
                        Required = fileParameters.Select(p => p.Name).ToHashSet()

                    }
                }
            }
            };
        }
    }

}
