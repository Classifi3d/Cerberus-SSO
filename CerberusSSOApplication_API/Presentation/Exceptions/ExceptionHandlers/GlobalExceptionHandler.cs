using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Presentation.Exceptions.ExceptionHandlers;

public class GlobalExceptionHandler() : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {

        var traceId = httpContext.TraceIdentifier;
        var request = httpContext.Request;

        Log.ForContext("TraceId", traceId)
               .ForContext("RequestPath", request.Path)
               .ForContext("QueryString", request.QueryString.ToString())
               .ForContext("Method", request.Method)
               .ForContext("User", httpContext.User?.Identity?.Name ?? "anonymous")
               .Error(exception, "Unhandled exception occurred");

        var problemDetails = new ProblemDetails
        {
            Title = "An unhandled error occurred",
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
