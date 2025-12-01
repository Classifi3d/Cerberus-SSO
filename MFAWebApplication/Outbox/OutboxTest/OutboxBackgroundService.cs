
//namespace MFAWebApplication.Outbox;

//public class OutboxBackgroundService(
//    IServiceScopeFactory serviceScopeFactory) : BackgroundService
//{

//    private const int OUTBOX_PROCESSOR_FREQUENCY = 5;
//    private const int MAX_THREADS = 5;


//    protected override async Task ExecuteAsync(CancellationToken cancelationToken)
//    {
//        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
//        using var linkedCts = CancellationTokenSource.
//            CreateLinkedTokenSource(cts.Token, cancelationToken);

//        var parallelOptions = new ParallelOptions()
//        {
//            MaxDegreeOfParallelism = MAX_THREADS,
//            CancellationToken = linkedCts.Token
//        };

//        try
//        {
//            await Parallel.ForEachAsync(
//                Enumerable.Range(0, MAX_THREADS),
//                parallelOptions,
//                async (_, token) =>
//                {
//                    await ProcessOutboxMessages(token);
//                }
//                ).ConfigureAwait(false);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine(ex);
//        }

//    }

//    private async Task ProcessOutboxMessages(CancellationToken cancellationToken) { 
//        using var scope = serviceScopeFactory.CreateScope();
//        var outboxProcessor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

//        while (!cancellationToken.IsCancellationRequested) {
//            int processedMessages = await outboxProcessor.ExecuteAsync(cancellationToken);

//            await Task.Delay(TimeSpan.FromSeconds(OUTBOX_PROCESSOR_FREQUENCY), cancellationToken);
//        }
//    }

//}
