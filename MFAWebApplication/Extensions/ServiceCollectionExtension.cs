using MessagePack;
using MFAWebApplication.Abstraction;
using MFAWebApplication.Abstraction.Messaging;
using MFAWebApplication.Abstraction.Repository;
using MFAWebApplication.Abstraction.UnitOfWork;
using MFAWebApplication.Context;
using MFAWebApplication.DTOs;
using MFAWebApplication.Entities.User;
using MFAWebApplication.Kafka;
using MFAWebApplication.Outbox;
using MFAWebApplication.Projections;
using MFAWebApplication.Services;
using System.Reflection;

namespace MFAWebApplication.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        // Infrastructure
        services.AddScoped<UnitOfWork<WriteDbContext>>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IReadModelRepository<>), typeof(ReadModelRepository<>));

        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .AsSelf()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .AsSelf()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .AsSelf()
                .WithScopedLifetime()
        );

        services.AddScoped<IMediator>(sp => new Mediator(Assembly.GetExecutingAssembly(), sp));

        // Services
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddSingleton(MapperConfiguration.InitializeAutomapper());
        services.AddMemoryCache();

        // Messaging Queue
        // Sender 
        services.AddSingleton<KafkaProducerService>();
        services.AddSingleton<OutboxProcessorService>();
        services.AddHostedService(sp => sp.GetRequiredService<OutboxProcessorService>());

        MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

        // Receiver 
        services.AddHostedService<KafkaConsumerService>();
        services.AddScoped<UserUpsertProjector>();
        services.AddScoped<UserDeleteProjector>();

        var projectorMap = new Dictionary<string, Type>
        {
            [nameof(UserUpsertEvent)] = typeof(UserUpsertProjector),
            [nameof(UserDeletedEvent)] = typeof(UserDeleteProjector),
        };
        services.AddSingleton<IDictionary<string, Type>>(projectorMap);


        // Doesn't stop the program in case of background events crashing
        services.Configure<HostOptions>(opts =>
        {
            opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });

        return services;
    }
}
