using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class KafkaExtension
{
    public static IServiceCollection AddKafkaProducer(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IProducer<Null, string>>(_ =>
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                LingerMs = 50,
                Acks = Acks.Leader
            };

            return new ProducerBuilder<Null, string>(producerConfig).Build();

        });

        return services;
    }

}
