using MFAWebApplication.Context;
using Microsoft.EntityFrameworkCore;

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
        // !!!!!!!!
        //builder.Services.AddDbContext<ReadDbContext>(options =>
        //{
        //    options.UseMongo(builder.Configuration); // or your existing setup
        //});


        // Read Database MongoDB
        builder.Services.AddSingleton<ReadDbContext>();


        return builder;
    }
}

