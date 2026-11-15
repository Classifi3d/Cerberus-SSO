using Application.Email.Abstraction;
using Application.Email.Contracts;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Kafka;

public class KafkaEmailConsumerService : BackgroundService
{
    private const int CONSUMER_PROCESS_FREQUENCY = 3;
    private const int BATCH_SIZE = 1000;


    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private readonly ILogger<KafkaEmailConsumerService> _logger;


    public KafkaEmailConsumerService(
        IServiceProvider serviceProvider,
        IConfiguration config,
        ILogger<KafkaEmailConsumerService> logger
    )
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {

        var topic = _config["Kafka:Topics:UserEmails"] ?? string.Empty;
        var section = _config.GetSection("Kafka:Consumers:Email");

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
            "Email consumer started. Group: {GroupId}, Topic: {Topic}",
            consumerConfig.GroupId, topic);

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
                    _logger.LogError(exception, "Email consumer error: {Reason}", exception.Error.Reason);
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

        EmailMessageEvent? emailEvent;
        try
        {
            emailEvent = JsonSerializer.Deserialize<EmailMessageEvent>(result.Message.Value);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(
                exception,
                "Skipping malformed email event at offset {Offset}",
                result.Offset);

            CommitSafely(consumer, result);
            return true;
        }

        if (emailEvent is null)
        {
            _logger.LogWarning(
                "Skipping invalid email event at offset {Offset}",
                result.Offset);

            CommitSafely(consumer, result);
            return true;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var emailSender = scope.ServiceProvider
                       .GetRequiredService<IEmailSender>();

            await emailSender.SendAsync(emailEvent, cancellationToken);

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process email event at offset {Offset}", result.Offset);
            return false;
        }
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
            _logger.LogError(exception, "Failed to commit email offset {Offset}", result.Offset);
        }
    }
}
