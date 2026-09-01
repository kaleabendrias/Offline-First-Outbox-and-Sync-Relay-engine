using System.Text.Json;
using System.Text.Json.Nodes;
using FluentValidation;
using SyncRelay.Core.Messages;

namespace SyncRelay.Api.Validators;


public class SyncMutationsMessagesValidator: AbstractValidator<SyncMutationMessages>
{
    public SyncMutationsMessagesValidator()
    {
        RuleFor(x => x.Mutations).NotEmpty().WithMessage("Mutation list cannot be empty.");
        RuleForEach(x => x.Mutations)
            .SetValidator(new SyncMutationMessageValidator());
    }
}

public class SyncMutationMessageValidator: AbstractValidator<SyncMutationMessage>
{
    public SyncMutationMessageValidator()
    {
        RuleFor(x => x.MutationId).NotEqual(Guid.Empty).WithMessage("Mutation ID cannot be empty.");
        RuleFor(x => x.Payload)
            .NotEmpty()
            .WithMessage("Mutation payload cannot be empty.")
            .Must(BeAValidJson)
            .WithMessage("Mutation payload must be a valid JSON string.");
    }

    private bool BeAValidJson(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return false;
        try
        {
            JsonDocument.Parse(payload);
            return true;
        }
        catch
        {
            return false;
        }
    }
}