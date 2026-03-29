
using Domain.Entities.User;
using Infrastructure.Kafka;
using Infrastructure.Outbox;
using Infrastructure.Projections;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Extensions;

public static class MessagingExtension 
{
    public static IServiceCollection AddMessagingServices(this IServiceCollection services)
    {
        // Serializer
        MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
            .WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);

        // Sender 
        services.AddSingleton<KafkaProducerService>();
        services.AddSingleton<OutboxProcessorService>();
        services.AddHostedService(sp => sp.GetRequiredService<OutboxProcessorService>());

        // Doesn't stop the program in case of background events crashing
        services.Configure<HostOptions>(opts =>
        {
            opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });


        // Receiver 
        services.AddHostedService<KafkaConsumerService>();
        services.AddScoped<UserUpsertProjector>();
        services.AddScoped<UserDeleteProjector>();

        var projectorMap = new Dictionary<string, Type>
        {
            [nameof(UserUpsertEvent)] = typeof(UserUpsertProjector),
            [nameof(UserDeleteEvent)] = typeof(UserDeleteProjector),
        };
        services.AddSingleton<IDictionary<string, Type>>(projectorMap);


        return services;
    }

}
