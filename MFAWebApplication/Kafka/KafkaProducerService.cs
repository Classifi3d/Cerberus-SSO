using Confluent.Kafka;
using MFAWebApplication.Outbox;
using System.Text;

namespace MFAWebApplication.Kafka;

public class KafkaProducerService
{
    private readonly IProducer<Null, byte[]> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaProducerService> _logger;

    private const int BATCH_SIZE = 1000;

    public KafkaProducerService(
        IConfiguration config, 
        ILogger<KafkaProducerService> logger)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            LingerMs = 50,
            BatchNumMessages = BATCH_SIZE,
            Acks = Acks.Leader
        };
        _topic = config["Kafka:Topic"];
        _producer = new ProducerBuilder<Null, byte[]>(producerConfig).Build();
        _logger = logger;
    }

    public async Task ProduceAsync(OutboxMessage message)
    {
        _logger.LogDebug("Producing Kafka message {MessageId} of type " +
            "{MessageType} to topic {Topic}", message.Id, message.Type, _topic);

        var kafkaMessage = new Message<Null, byte[]>
        {
            Value = message.Payload,
        };
        kafkaMessage.Headers ??= new Headers();
        kafkaMessage.Headers.Add("type", Encoding.UTF8.GetBytes(message.Type));
        kafkaMessage.Headers.Add("outbox-id", Encoding.UTF8.GetBytes(message.Id.ToString()));

        await _producer.ProduceAsync(_topic, kafkaMessage);
    }
}
