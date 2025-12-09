
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
    private const int PRODUCE_CONCURRENCY = 64;

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
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await ProcessPendingMessagesAsync(cancellationToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<WriteDbContext>();

        var outboxMessages = await database.OutboxMessages
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


        if (outboxMessages.Count == 0) return;

        var successIds = new ConcurrentBag<Guid>();
        var sem = new SemaphoreSlim(PRODUCE_CONCURRENCY);

        var produceTasks = outboxMessages.Select(async msg =>
        {
            await sem.WaitAsync(cancellationToken);
            try
            {
                try
                {
                    await _kafka.ProduceAsync(msg);
                    successIds.Add(msg.Id);
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

        var producedList = successIds.Distinct().ToList();
        if (producedList.Count > 0)
        {
            await database.OutboxMessages
                .Where(m => producedList.Contains(m.Id))
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(m => m.ProcessedAt, DateTime.UtcNow), cancellationToken);
        }
    }
}
