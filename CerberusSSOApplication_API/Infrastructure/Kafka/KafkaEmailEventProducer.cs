using Application.Email.Abstraction;
using Application.Email.Contracts;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Infrastructure.Kafka;

public class KafkaEmailEventProducer : IEmailEventProducer
{
    public readonly IProducer<Null, byte[]> _producer;
    public readonly string _topic;

    public KafkaEmailEventProducer(IProducer<Null, byte[]> producer, IConfiguration config)
    {
        _producer = producer;
        _topic = config["Kafka:Topics:UserEmails"] ?? string.Empty;
    }

    public Task QueueEmailEventAsync(EmailMessageEvent emailEvent, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(emailEvent);

        var kafkaMessage = new Message<Null, byte[]> { Value = payload };

        return _producer.ProduceAsync(_topic, kafkaMessage, cancellationToken);
    }
}
