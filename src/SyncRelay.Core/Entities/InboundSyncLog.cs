namespace SyncRelay.Core.Entities;

public class InboundSyncLog
{
    public Guid MutationId {get; set;}
    public Guid ClientId {get; set;}
    public DateTimeOffset ReceivedAt {get; set;}
}