using Application.Projections.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Infrastructure.Kafka;

public class KafkaConsumerService : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly string ?_topic;
    private readonly IConsumer<Null, byte[]> _consumer;
    private readonly IDictionary<string, Type> _projectorTypes;
    private bool _appStarted = false;
    private readonly ILogger<KafkaConsumerService> _logger;

    private const int CONSUMER_PROCESS_FREQUENCY = 3;
    private const int BATCH_SIZE = 1000;

    public KafkaConsumerService(
        IHostApplicationLifetime appLifetime,
        IServiceProvider serviceProvider,
        IConfiguration config,
        IDictionary<string, Type> projectorTypes,
        ILogger<KafkaConsumerService> logger)
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
            MaxPartitionFetchBytes = 4 * 1024 * 1024, // 4 MB per partition
            // Queueing / batching
            QueuedMinMessages = 1000,              // Minimum number buffered
            QueuedMaxMessagesKbytes = 51200,       // 50 MB local queue
            // Heartbeat / session
            SessionTimeoutMs = 10000,
            MaxPollIntervalMs = 300000,
        };
        _topic = config["Kafka:Topic"];

        _consumer = new ConsumerBuilder<Null, byte[]>(consumerConfig).Build();
        _projectorTypes = projectorTypes;
        _logger = logger;

        _appLifetime.ApplicationStarted.Register(() =>
        {
            _appStarted = true;
        });
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KafkaConsumerService is trying to start. Listening to topic: {topic}", _topic);
        while (!_appStarted && !stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("KafkaConsumerService cannot be started yet. " +
                "Waiting for application to start.  Listening to topic: {topic}", _topic);

            await Task.Delay(1000, stoppingToken);
        }
        _consumer.Subscribe(_topic);
        _logger.LogInformation("KafkaConsumerService started. Listening to topic: {topic}", _topic);

        // Starting to consume messages
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

                // Get the event type from Kafka headers
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
                    _logger.LogCritical("WARNING: host requested shutdown during processing");

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
                        _logger.LogInformation("Batch of size {BatchSize} processed", BATCH_SIZE);
                    }
                    catch (KafkaException exception)
                    {
                        _logger.LogError("Error processing Kafka batch {Exception}", exception);
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