using Confluent.Kafka;
using MFAWebApplication.Projections;
using System;
using System.Text;

public class KafkaConsumerService : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _topic;
    private readonly IConsumer<Null, byte[]> _consumer;
    private readonly IDictionary<string, Type> _projectorTypes;
    private bool _appStarted = false;

    private const int CONSUMER_PROCESS_FREQUENCY = 3;
    private const int BATCH_SIZE = 100;

    public KafkaConsumerService(
        IHostApplicationLifetime appLifetime,
        IServiceProvider serviceProvider,
        IConfiguration config,
        IDictionary<string, Type> projectorTypes)
    {
        _appLifetime = appLifetime;
        _serviceProvider = serviceProvider;
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "read-db-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            // Fetch settings
            FetchWaitMaxMs = 50,                   // Wait max 50ms for data
            FetchMinBytes = 1,                     // Immediately deliver messages
            MaxPartitionFetchBytes = 4 * 1024 * 1024, // 4 MB per partition
            // Queueing / batching
            QueuedMinMessages = 1000,              // Minimum buffered
            QueuedMaxMessagesKbytes = 51200,       // 50 MB local queue
            // Heartbeat / session
            SessionTimeoutMs = 10000,
            MaxPollIntervalMs = 300000,
        };
        _topic = config["Kafka:Topic"];

        _consumer = new ConsumerBuilder<Null, byte[]>(consumerConfig).Build();
        _projectorTypes = projectorTypes;

        _appLifetime.ApplicationStarted.Register(() =>
        {
            _appStarted = true;
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"KafkaConsumerService started. Listening to topic: {_topic}");
        _consumer.Subscribe(_topic);

        while (!_appStarted && !stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        int processedSinceCommit = 0;
        var lastCommitTime = DateTime.UtcNow;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<Null, byte[]> result;

                try
                {
                    result = _consumer.Consume(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (result?.Message?.Value == null)
                    continue;

                string? type = null;
                var headers = result.Message.Headers;
                if (headers != null)
                {
                    var typeHeader = headers.GetLastBytes("type");
                    if (typeHeader != null)
                    {
                        type = Encoding.UTF8.GetString(typeHeader);
                    }
                }

                if (string.IsNullOrEmpty(type))
                {
                    _consumer.Commit(result);
                    continue;
                }

                if (!_projectorTypes.TryGetValue(type, out var projType))
                {
                    _consumer.Commit(result);
                    continue;
                }
                if (stoppingToken.IsCancellationRequested)
                    Console.WriteLine("WARNING: host requested shutdown during processing");

                using var scope = _serviceProvider.CreateScope();
                var projector = (IEventProjector)scope.ServiceProvider.GetRequiredService(projType);

                var payload = result.Message.Value;
                await projector.ProjectAsync(payload, CancellationToken.None);

                processedSinceCommit++;
                if ((processedSinceCommit >= BATCH_SIZE) ||
                    (DateTime.UtcNow - lastCommitTime) >= TimeSpan.FromSeconds(CONSUMER_PROCESS_FREQUENCY))
                {
                    try
                    {
                        _consumer.Commit();
                        lastCommitTime = DateTime.UtcNow;
                        processedSinceCommit = 0;
                        Console.WriteLine($"Batch of size {BATCH_SIZE} commited");
                    }
                    catch (KafkaException e)
                    {
                        Console.WriteLine($"Batch commited error: {e}");
                    }
                }
            }
        }
        finally
        {
            try
            {
                _consumer.Commit();
            }
            catch { }
            _consumer.Close();
        }
    }
}