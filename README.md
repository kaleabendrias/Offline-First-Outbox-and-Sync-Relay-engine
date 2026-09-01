# Offline-First Outbox & Sync Relay Engine

A high-throughput, zero-data-loss sync engine for offline-first applications, built with **.NET 9**, **Entity Framework Core**, and **PostgreSQL**.

## What It Does

Edge clients (mobile, desktop, IoT) submit data mutations — even while offline. The engine accepts them instantly via a non-blocking in-memory channel, durably persists them to an outbox, and relays them upstream to a central API gateway with automatic retries (exponential backoff + jitter). No mutation is ever lost.

## How It Works

1. **Ingest** — Clients `POST /api/sync` with batched mutations; the API returns `202 Accepted` immediately.
2. **Persist** — A background worker batches and deduplicates writes (by `mutation_id`) into PostgreSQL.
3. **Relay** — Pending outbox events are polled and dispatched upstream with resilient retry policies.
4. **Clean up** — Processed events are periodically purged.

## Tech Stack

- .NET 9 / ASP.NET Core
- Entity Framework Core + PostgreSQL
- `System.Threading.Channels` for async ingestion
- Polly for resilient upstream dispatch
- Docker Compose for local infrastructure

## Project Structure

```
src/
├── SyncRelay.Core/            # Domain entities, enums, messages
├── SyncRelay.Infrastructure/  # EF Core DbContext, migrations, workers
└── SyncRelay.Api/             # HTTP API and host
```

## Getting Started

```bash
# Start PostgreSQL
docker compose up -d

# Run the API
dotnet run --project src/SyncRelay.Api
```

## License

Open source — see [LICENSE](LICENSE) for details.
