
using MFAWebApplication.Context;
using MFAWebApplication.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Threading;
using System.Collections.Concurrent;

namespace MFAWebApplication.Outbox;

public class OutboxProcessorService : BackgroundService
{
    private const int OUTBOX_PROCESSOR_FREQUENCY = 3;
    private const int BATCH_SIZE = 1000;
    private const int THREAD_COUNT = 4;

    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaProducerService _kafka;
    private readonly AsyncAutoResetEvent _signal = new(false);
    private readonly ILogger<OutboxProcessorService> _logger;

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        KafkaProducerService kafka,
        ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _kafka = kafka;
        _logger = logger;
    }

    public void NotifyNewOutboxMessage()
    {
        _signal.Set();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(OUTBOX_PROCESSOR_FREQUENCY));

        _logger.LogInformation("Outbox processor started with frequency " +
            "{FrequencySeconds}s", OUTBOX_PROCESSOR_FREQUENCY);

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await ProcessPendingMessagesAsync(cancellationToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<WriteDbContext>();

        // Select pending outbox messages
        List<OutboxMessage> outboxMessages;
        try
        {
            outboxMessages = await database.OutboxMessages
            .FromSqlRaw(
                $"""
                SELECT *
                FROM "OutboxMessages"
                WHERE "ProcessedAt" IS NULL
                ORDER BY "CreatedAt"
                LIMIT {BATCH_SIZE}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed querying outbox messages");
            return;
        }

        if (outboxMessages.Count == 0)
        {
            _logger.LogInformation("No pending outbox messages found");
            return;
        }

        // Publish concurrently
        var successIds = new ConcurrentBag<Guid>();
        var sem = new SemaphoreSlim(THREAD_COUNT);
        var produceTasks = outboxMessages.Select(async msg =>
        {
            await sem.WaitAsync(cancellationToken);
            try
            {
                try
                {
                    await _kafka.ProduceAsync(msg);
                    successIds.Add(msg.Id);
                    _logger.LogDebug("Outbox message {MessageId} " +
                        "successfully produced to Kafka", msg.Id);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Produce failed for Outbox {msg.Id}: {e.Message}");
                }
            }
            finally
            {
                sem.Release();
            }
        }).ToList();

        await Task.WhenAll(produceTasks);

        // Apply updates
        var producedList = successIds.Distinct().ToList();
        if (producedList.Count > 0)
        {
            try
            {
                await database.OutboxMessages
                    .Where(m => producedList.Contains(m.Id))
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(m => m.ProcessedAt, DateTime.UtcNow), cancellationToken);

                _logger.LogInformation("Marked {Count} outbox messages as processed", producedList.Count);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed updating ProcessedAt for " +
                    "{Count} outbox messages", producedList.Count);
            }
        }
    }
}
