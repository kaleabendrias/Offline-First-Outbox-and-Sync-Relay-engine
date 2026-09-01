using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SyncRelay.Core.Enums;
using SyncRelay.Infrastructure.Data;

namespace SyncRelay.Infrastructure.Workers;


public class SyncRelayWorker(
    IServiceScopeFactory _serviceScopeFactory,
    ILogger<SyncRelayWorker> _logger
) : BackgroundService
{
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SyncRelay worker running at: {Time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
                {
                    
                using var scope = _serviceScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SyncDbContext>();

                var pendingEvents = await db.outboxEvents
                    .Where(e => (e.Status == OutboxEventStatus.Pending || e.Status == OutboxEventStatus.Failed) && (e.NextRetryAt == null || e.NextRetryAt <= DateTimeOffset.UtcNow))
                    .OrderBy(e => e.CreatedAt)
                    .Take(100)
                    .ToListAsync(stoppingToken);

                if (pendingEvents.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                foreach (var outboxEvent in pendingEvents)
                {
                    outboxEvent.RetryCount = 0;
                    try
                    {
                        // this is where the real processing happends, probably some http request or sth
                        outboxEvent.Status = OutboxEventStatus.Processed;
                        outboxEvent.ProcessedAt = DateTimeOffset.UtcNow;
                        outboxEvent.NextRetryAt = null;

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing outbox event {EventId}", outboxEvent.Id);
                        outboxEvent.Status = OutboxEventStatus.Failed;
                        outboxEvent.RetryCount++;

                        double baseSeconds = Math.Pow(2, Math.Min(outboxEvent.RetryCount, 6));
                        double jitter = _random.NextDouble() * 3.0; // Add 0 to 3 seconds of random jitter
                        double totalDelaySeconds = Math.Min(baseSeconds + jitter, 300);

                        outboxEvent.NextRetryAt = DateTimeOffset.Now.AddSeconds(totalDelaySeconds);
                    }
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SyncRelay worker loop");
                }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

    }
}
