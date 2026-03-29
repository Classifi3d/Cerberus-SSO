using Application.Abstraction.Services;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Infrastructure.Extensions;

public static class CachingExtension
{
    public static IHostApplicationBuilder AddCachingServices(this IHostApplicationBuilder builder)
    {
        // Redis Cache Database 
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(
                builder.Configuration.GetConnectionString("Redis")!);
            configuration.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(configuration);
        });

        builder.Services.AddMemoryCache();

        builder.Services.AddScoped<ICacheService, CacheService>();

        return builder;

    }

}
