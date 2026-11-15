using Confluent.Kafka;
using Infrastructure.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Infrastructure.Kafka;

public class KafkaOutboxEventProducer
{
    private readonly IProducer<Null, byte[]> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaOutboxEventProducer> _logger;

    public KafkaOutboxEventProducer(
        IProducer<Null, byte[]> producer,
        IConfiguration config,
        ILogger<KafkaOutboxEventProducer> logger)
    {
        _producer = producer;
        _topic = config["Kafka:Topics:UserOutbox"] ?? string.Empty;
        _logger = logger;
    }

    public async Task ProduceAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Producing Kafka message {MessageId} of type " +
            "{MessageType} to topic {Topic}", message.Id, message.Type, _topic);

        var kafkaMessage = new Message<Null, byte[]>
        {
            Value = message.Payload,
            Headers = new Headers
            {
                { "type", Encoding.UTF8.GetBytes(message.Type) },
                { "outbox-id", Encoding.UTF8.GetBytes(message.Id.ToString()) }
            }
        };

        await _producer.ProduceAsync(_topic, kafkaMessage, cancellationToken);
    }
}
