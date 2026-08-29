using Application.Abstraction;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Repository;
using Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Infrastructure.Extensions;

public static class InfrastructureExtension
{
    public static IHostApplicationBuilder AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        // Write Database PostgreSQL
        builder.Services.AddDbContext<WriteDbContext>(
            options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("PostgreSQL_Write_Connection_String")
                )
        );
        //builder.Services.AddScoped<UnitOfWork<WriteDbContext>>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork<WriteDbContext>>();

        // Read Database MongoDB
        builder.Services.AddSingleton<ReadDbContext>();

        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped(typeof(IReadModelRepository<>), typeof(ReadModelRepository<>));


        // Read Database PostgreSQL
        //builder.Services.AddDbContext<ReadDbContext>(
        //    options =>
        //        options.UseNpgsql(
        //            builder.Configuration.GetConnectionString("PostgreSQL_Read_Connection_String")
        //        )
        //);

        return builder;
    }
}