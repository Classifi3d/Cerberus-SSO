using MFAWebApplication.Exceptions.ExceptionHandlers;

namespace MFAWebApplication.Extensions;

public static class ExceptionsExtension
{
    public static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
    {

        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}