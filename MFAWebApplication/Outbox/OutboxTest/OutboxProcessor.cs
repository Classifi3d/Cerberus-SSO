//using MFAWebApplication.Context;
//using MFAWebApplication.Kafka;
//using Microsoft.EntityFrameworkCore;
//using System.Collections.Concurrent;


//namespace MFAWebApplication.Outbox;

//public class OutboxProcessor
//{
//    private const int BATCH_SIZE = 100;
//    private static readonly ConcurrentDictionary<string, Type> TypeCache = new();

//    private readonly WriteDbContext _dbContext;
//    private readonly KafkaProducerService _kafkaProducer;

//    public OutboxProcessor(
//        WriteDbContext dbContext,
//        KafkaProducerService kafkaProducer
//    )
//    {
//        _dbContext = dbContext;
//        _kafkaProducer = kafkaProducer;
//    }

//    public async Task<int> ExecuteAsync(
//        CancellationToken cancellationToken = default)
//    {
//        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

//        // Step 1. Select pending messages with EF Core + raw SQL (skip locked)
//        var messages = await _dbContext.OutboxMessages
//            .FromSqlRaw(
//                """
//                SELECT ID, TYPE, PAYLOAD, PROCESSED, CREATEDAT
//                FROM OUTBOXMESSAGES
//                WHERE PROCESSED IS FALSE
//                ORDER BY CREATEDAT
//                LIMIT {0}
//                FOR UPDATE SKIP LOCKED
//                """, BATCH_SIZE)
//            .AsNoTracking()
//            .ToListAsync(cancellationToken);

//        var publishTasks = new List<Task>();
//        var updateQueue = new ConcurrentQueue<OutboxUpdate>();

//        // Step 2. Publish concurrently
//        foreach (var message in messages)
//        {
//            publishTasks.Add(PublishMessageAsync(message, updateQueue, cancellationToken));
//        }

//        await Task.WhenAll(publishTasks);

//        // Step 3. Apply updates
//        foreach (var update in updateQueue)
//        {
//            await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
//                UPDATE outbox_messages
//                SET processed_at = {update.ProcessedAt},
//                WHERE id = {update.Id}
//            ", cancellationToken);
//        }

//        await transaction.CommitAsync(cancellationToken);

//        return messages.Count;
//    }

//    private async Task PublishMessageAsync(
//        OutboxMessage message,
//        ConcurrentQueue<OutboxUpdate> updateQueue,
//        CancellationToken cancellationToken)
//    {
//        try
//        {
//            await _kafkaProducer.ProduceAsync(message);

//            updateQueue.Enqueue(new OutboxUpdate
//            {
//                Id = message.Id,
//                ProcessedAt = DateTime.UtcNow,
//            });
//        }
//        catch
//        {
//            Console.WriteLine($"Error Processing {message.Id} at {DateTime.UtcNow}");
//        }
//    }
//}
