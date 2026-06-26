using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Backend.API.Middlewares
{
    public class ConfigureApiBehaviorOptions : IConfigureOptions<ApiBehaviorOptions>
    {
        public void Configure(ApiBehaviorOptions options)
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = string.Join(" | ", context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                string friendlyDetail = "An error occurred while processing the data packet.";

                if (errors.Contains("trailing comma"))
                    friendlyDetail = "The JSON format is incorrect: You included an extra (unnecessary) comma at the end of the data block. Please remove the final comma and try again.";
                
                else if (errors.Contains("invalid after a value") || errors.Contains("Expected either"))
                    friendlyDetail = "Invalid JSON format: You left a field (e.g., ‘content’) empty, or there is a typo in the curly braces or commas. Please check the JSON structure.";
                
                else if (errors.Contains("could not be mapped"))
                    friendlyDetail = "The request contains a property that is not supported by the system, is undefined, or is extra.";
                
                else if (!string.IsNullOrEmpty(errors))
                    friendlyDetail = errors;

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid Request / Format Error",
                    Detail = friendlyDetail,
                    Instance = context.HttpContext.Request.Path
                };

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        }
    }
}
