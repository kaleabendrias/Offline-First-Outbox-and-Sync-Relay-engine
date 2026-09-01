using SyncRelay.Core.Enums;

namespace SyncRelay.Core.Entities;

public class OutboxEvents
{
    public Guid Id {get; set;}
    public required string Payload {get; set;}
    public int RetryCount {get; set;}
    public DateTimeOffset? NextRetryAt {get; set;}
    public OutboxEventStatus Status {get; set;}
    public DateTimeOffset CreatedAt {get; set;}
    public DateTimeOffset ProcessedAt {get; set;}
}