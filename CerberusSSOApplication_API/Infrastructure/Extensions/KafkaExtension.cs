using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class KafkaExtension
{
    // Generic Kafka primitives (raw producer/consumer building blocks)
    public static IServiceCollection AddKafkaProducer(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IProducer<Null, byte[]>>(_ =>
        {
            var batchSize = config.GetValue<int?>("Kafka:ProducerBatchSize") ?? 1000;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"] ?? string.Empty,
                LingerMs = 50,
                Acks = Acks.Leader,
                BatchSize = batchSize
            };

            return new ProducerBuilder<Null, byte[]>(producerConfig).Build();
        });

        return services;
    }
}
