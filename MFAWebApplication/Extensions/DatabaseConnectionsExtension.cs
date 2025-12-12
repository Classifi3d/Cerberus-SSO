using MFAWebApplication.Context;
using MFAWebApplication.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace MFAWebApplication.Extensions;

public static class DatabaseConnectionsExtension
{
    public static IHostApplicationBuilder AddDatabaseConnections(this IHostApplicationBuilder builder)
    {
        // Write Database PostgreSQL
        builder.Services.AddDbContext<WriteDbContext>(
            options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("PostgreSQL_Write_Connection_String")
                )
        );

        // Read Database PostgreSQL
        //builder.Services.AddDbContext<ReadDbContext>(
        //    options =>
        //        options.UseNpgsql(
        //            builder.Configuration.GetConnectionString("PostgreSQL_Read_Connection_String")
        //        )
        //);

        // Read Database MongoDB
        builder.Services.AddSingleton<ReadDbContext>();

        // Redis Cache Database 
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(
                builder.Configuration.GetConnectionString("Redis"));
            configuration.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(configuration);
        });
        builder.Services.AddScoped<ICacheService, CacheService>();

        return builder;
    }
}

