
using Confluent.Kafka;
using MFAWebApplication.Context;
using MFAWebApplication.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Threading;

namespace MFAWebApplication.Outbox;

public class OutboxProcessorService : BackgroundService
{
    private const int OUTBOX_PROCESSOR_FREQUENCY = 3;
    private const int BATCH_SIZE = 100;

    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaProducerService _kafka;
    private readonly AsyncAutoResetEvent _signal = new(false);

    public OutboxProcessorService(IServiceProvider serviceProvider, KafkaProducerService kafka)
    {
        _serviceProvider = serviceProvider;
        _kafka = kafka;
    }

    public void NotifyNewOutboxMessage()
    {
        _signal.Set();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(OUTBOX_PROCESSOR_FREQUENCY));
        while (!cancellationToken.IsCancellationRequested)
        {

            var signalTask = _signal.WaitAsync(cancellationToken);
            var timerTask = periodicTimer.WaitForNextTickAsync(cancellationToken).AsTask();

            var completed = await Task.WhenAny(signalTask, timerTask);
            if (completed.IsCanceled || cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await ProcessPendingMessageAsync(cancellationToken);

        }
    }

    private async Task ProcessPendingMessageAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<WriteDbContext>();

        var outboxMessages = await database.OutboxMessages
            .Where(m => !m.Processed)
            .OrderBy(m => m.CreatedAt)
            .Take(BATCH_SIZE)
            .ToListAsync(cancellationToken);


        if (outboxMessages.Count == 0)
            return;

        foreach (var message in outboxMessages)
        {
            try
            {
                await _kafka.ProduceAsync(message);
                message.Processed = true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Outbox Error: {e.Message}");
            }
        }

        await database.SaveChangesAsync(cancellationToken);
    }
}
