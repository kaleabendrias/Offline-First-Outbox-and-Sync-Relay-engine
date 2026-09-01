namespace SyncRelay.Core.Messages;

public record SyncMutationMessage(
    Guid MutationId,
    string ClientId,
    string AggregateType,
    string AggregateId,
    string Payload
);

public record SyncMutationMessages(List<SyncMutationMessage> Mutations);