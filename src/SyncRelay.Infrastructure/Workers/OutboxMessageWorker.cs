using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SyncRelay.Core.Entities;
using SyncRelay.Core.Messages;
using SyncRelay.Infrastructure.Data;



namespace SyncRelay.Infrastructure.Workers;


public class OutboxIngestionWorker(
    Channel<SyncMutationMessage> channel,
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxIngestionWorker> logger): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await channel.Reader.WaitToReadAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SyncDbContext>();
            var batch = new List<OutboxEvents>();
            var deduplicationLogs = new List<InboundSyncLog>();
            
            while (batch.Count < 250 && channel.Reader.TryRead(out var msg))
            {
                bool isDuplicate =  db.inboundSyncLogs.Any(x => x.MutationId == msg.MutationId);
                if (isDuplicate)
                {
                    continue;
                }

                var outboxEvent = new OutboxEvents
                {
                    Id = Guid.NewGuid(),
                    RetryCount = 0,
                    Payload = msg.Payload,
                    Status = Core.Enums.OutboxEventStatus.Pending,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                batch.Add(outboxEvent);

                var deduplicationLogItem = new InboundSyncLog
                {
                    MutationId = outboxEvent.Id,
                    ClientId = Guid.Parse(msg.ClientId),
                    ReceivedAt = DateTimeOffset.UtcNow
                };
                deduplicationLogs.Add(deduplicationLogItem);
            }
            if (batch.Count > 0)
            {
                await db.inboundSyncLogs.AddRangeAsync(deduplicationLogs, stoppingToken);
                await db.outboxEvents.AddRangeAsync(batch, stoppingToken);
                await db.SaveChangesAsync(stoppingToken);

                logger.LogInformation("Flushed batch of {Count} events to Outbox Store.", batch.Count);
            }
        }
    }
};