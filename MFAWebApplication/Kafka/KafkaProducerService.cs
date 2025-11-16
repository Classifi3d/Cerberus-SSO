using Confluent.Kafka;
using MFAWebApplication.Outbox;
using System.Text;

namespace MFAWebApplication.Kafka;

public class KafkaProducerService
{
    private readonly IProducer<Null, byte[]> _producer;
    private readonly string _topic;

    private const int BATCH_SIZE = 100;

    public KafkaProducerService(
        IConfiguration config)
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
    }

    public async Task ProduceAsync(OutboxMessage message)
    {
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
