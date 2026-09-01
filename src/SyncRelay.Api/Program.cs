using System.Threading.Channels;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Scalar.AspNetCore;
using SyncRelay.Api.Validators;
using SyncRelay.Core.Messages;
using SyncRelay.Infrastructure.Data;
using SyncRelay.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton(Channel.CreateUnbounded<SyncMutationMessage>());

builder.Services.AddHostedService<OutboxIngestionWorker>();
builder.Services.AddHostedService<SyncRelayWorker>();

builder.Services.AddValidatorsFromAssemblyContaining<SyncMutationsMessagesValidator>();

builder.Services.AddDbContext<SyncDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
var dataSource = dataSourceBuilder.Build();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapPost("api/sync", async (
    [FromBody] SyncMutationMessages request, Channel<SyncMutationMessage> channel,
    IValidator<SyncMutationMessages> validator,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    var validationResult = await validator.ValidateAsync(request, ct);
    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    foreach (var mutations in request.Mutations)
    {
        await channel.Writer.WriteAsync(mutations, ct);
    }
    return Results.Accepted("request accepted.");
});

app.UseHttpsRedirection();

app.Run();
