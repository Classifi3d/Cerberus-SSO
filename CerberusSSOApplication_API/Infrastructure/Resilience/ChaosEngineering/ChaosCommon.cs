using Microsoft.AspNetCore.Http;
using System.Net;
using System.Reflection;

namespace Infrastructure.Resilience.ChaosEngineering;

public class ChaosCommon
{
    public const string CHAOS_ENABLED = "X-Chaos-Enabled";
    public const string CHAOS_LATENCY_INJECTION_RATE = "X-Chaos-Latency-Injection-Rate";
    public const string CHAOS_LATENCY_DURATION_MS= "X-Chaos-Latency-Duration-Ms";
    public const string CHAOS_FAULT_INJECTION_RATE= "X-Chaos-Fault-Injection-Rate";
    public const string CHAOS_FAULT_EXCEPTION= "X-Chaos-Fault-Exception";

    public static T GetPropertyFromHttpHeader<T>(HttpContext httpContext, string headerKey)
        where T : IParsable<T>
    {
        if (httpContext.Request.Headers.TryGetValue(headerKey, out var headerValue) && T.TryParse(headerValue!, null, out var result))
        {
            return result;   
        }

        return default!;
    }

    public static Exception GetExceptionFromString(string exceptionName)
    {
        var exceptionType = Assembly.GetExecutingAssembly()
            .GetTypes()
            //.Append(typeof(FluentValidation.ValidationException))
            .FirstOrDefault(t =>
                t.Name == exceptionName &&
                typeof(Exception).IsAssignableFrom(t)
            );

        try
        {
            return (Exception) Activator.CreateInstance(exceptionType!, "Chaos Injected Exception!");
        }
        catch
        {
            return new Exception("Chaos Injected Exception");
        }
    }

    public static HttpStatusCode IntToHttpStatusCode(int statusCodeNumber)
    {
        if (statusCodeNumber < 100 || statusCodeNumber > 599)
        {
            statusCodeNumber = StatusCodes.Status500InternalServerError;
        }
        return (HttpStatusCode)statusCodeNumber;
    }

    public static bool IsInEnabledUrlList(string[] urlList, string requestUrl)
    {
        if (urlList.Length == 0) // Enable all endpoints with empty list
        {
            return true;
        }

        return urlList.FirstOrDefault(u =>
            {
                var parts = u.Split('|', 2);
                return requestUrl.Contains(parts[0]) && requestUrl.Contains(parts[1]);
            }
        ) is not null;
    }
}
