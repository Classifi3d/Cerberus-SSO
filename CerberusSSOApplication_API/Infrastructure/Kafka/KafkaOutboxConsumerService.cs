using Application.Projections.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Infrastructure.Kafka;

public class KafkaOutboxConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private readonly IDictionary<string, Type> _projectorTypes;
    private readonly ILogger<KafkaOutboxConsumerService> _logger;

    private const int CONSUMER_PROCESS_FREQUENCY = 3;
    private const int BATCH_SIZE = 1000;

    public KafkaOutboxConsumerService(
        IServiceProvider serviceProvider,
        IConfiguration config,
        IDictionary<string, Type> projectorTypes,
        ILogger<KafkaOutboxConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _projectorTypes = projectorTypes;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var topic = _config["Kafka:Topics:UserOutbox"] ?? string.Empty;
        var section = _config.GetSection("Kafka:Consumers:Outbox");

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"],
            GroupId = section["GroupId"],
            AutoOffsetReset = section.GetValue("AutoOffsetReset", AutoOffsetReset.Earliest),
            EnableAutoCommit = section.GetValue("EnableAutoCommit", false),
            // Fetch settings
            FetchWaitMaxMs = section.GetValue("FetchWaitMaxMs", 50),
            MaxPartitionFetchBytes = section.GetValue("MaxPartitionFetchBytes", 4 * 1024 * 1024),
            // Queueing / batching
            QueuedMinMessages = section.GetValue("QueuedMinMessages", 1000),
            QueuedMaxMessagesKbytes = section.GetValue("QueuedMaxMessagesKbytes", 51200),
            // Heartbeat / session
            SessionTimeoutMs = section.GetValue("SessionTimeoutMs", 10000),
            MaxPollIntervalMs = section.GetValue("MaxPollIntervalMs", 300000)
        };

        using var consumer = new ConsumerBuilder<Null, byte[]>(consumerConfig).Build();
        consumer.Subscribe(topic);

        _logger.LogInformation(
            "Outbox consumer started. Group: {GroupId}, Topic: {Topic}",
            consumerConfig.GroupId, topic);

        // Starting to consume messages
        int processedSinceCommit = 0;
        var lastCommitTime = DateTime.UtcNow;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<Null, byte[]> result;

                try
                {
                    result = consumer.Consume(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException exception)
                {
                    _logger.LogError(exception, "Outbox consumer error: {Reason}", exception.Error.Reason);
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    continue;
                }


                bool processed;
                try
                {
                    processed = await TryProcessMessageAsync(consumer, result, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (!processed)
                {
                    // TODO: Add DLQ 

                    CommitSafely(consumer, result);
                    continue;
                }

                processedSinceCommit++;
                if (processedSinceCommit >= BATCH_SIZE ||
                    (DateTime.UtcNow - lastCommitTime) >= TimeSpan.FromSeconds(CONSUMER_PROCESS_FREQUENCY))
                {
                    CommitSafely(consumer, result);
                    _logger.LogInformation("Batch of size {BatchSize} processed", processedSinceCommit);
                    lastCommitTime = DateTime.UtcNow;
                    processedSinceCommit = 0;
                }
            }
        }
        finally
        {
            try
            {
                consumer.Commit();
            }
            catch { }
            consumer.Close();
        }
    }

    private async Task<bool> TryProcessMessageAsync(
        IConsumer<Null, byte[]> consumer,
        ConsumeResult<Null, byte[]> result,
        CancellationToken cancellationToken)
    {
        if (result?.Message?.Value is null)
        {
            return true;
        }

        string? type = GetEventTypesFromMessage(result.Message.Headers);

        if (string.IsNullOrEmpty(type))
        {
            CommitSafely(consumer, result);
            return true;
        }

        if (!_projectorTypes.TryGetValue(type, out var projectorType))
        {
            CommitSafely(consumer, result);
            return true;
        }

        // TODO: Create and add polly retry pipeline here
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var projector = (IEventProjector)scope.ServiceProvider
                .GetRequiredService(projectorType);

            var payload = result.Message.Value;
            await projector.ProjectAsync(payload, cancellationToken);

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process outbox event at offset {Offset}", result.Offset);
            return false;
        }
    }

    private static string? GetEventTypesFromMessage(Headers? headers)
    {
        var typeHeader = headers?.GetLastBytes("type");
        if (typeHeader is null)
        {
            return null;
        }
        return Encoding.UTF8.GetString(typeHeader);
    }

    private void CommitSafely(
        IConsumer<Null, byte[]> consumer,
        ConsumeResult<Null, byte[]> result)
    {
        try
        {
            consumer.Commit(result);
        }
        catch (KafkaException exception)
        {
            _logger.LogError(exception, "Failed to commit outbox offset {Offset}", result.Offset);
        }
    }
}
