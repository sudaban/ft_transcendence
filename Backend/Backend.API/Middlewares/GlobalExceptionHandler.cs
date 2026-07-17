using Backend.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Backend.API.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync
            (HttpContext httpContext
            , Exception exception
            , CancellationToken cancellationToken)
        {


            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            if (exception is BaseException baseException)
            {
                problemDetails.Status = baseException.StatusCode;
                problemDetails.Title = baseException.Title;
                problemDetails.Detail = exception.Message;
            }
            else
            {
                problemDetails.Status = (int)HttpStatusCode.InternalServerError; 
                problemDetails.Title = "System Server Error"; 
                problemDetails.Detail = "An unexpected situation occured while the transaction was being processed.";
            }

            httpContext.Response.StatusCode = (int)problemDetails.Status;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
