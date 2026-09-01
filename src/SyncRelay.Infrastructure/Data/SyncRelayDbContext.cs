using Microsoft.EntityFrameworkCore;
using SyncRelay.Core.Entities;

namespace SyncRelay.Infrastructure.Data;

public class SyncDbContext: DbContext
{
    public SyncDbContext(DbContextOptions<SyncDbContext> options) : base(options) {}
    public DbSet<OutboxEvents> outboxEvents { get; set; }

    public DbSet<InboundSyncLog> inboundSyncLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<OutboxEvents>(entity =>
        {
            entity.ToTable("outbox_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>();
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("idx_outbox_pending")
                .HasFilter("status IN ('Pending', 'Failed')");
        });

        modelBuilder.Entity<InboundSyncLog>(entity =>
        {
            entity.ToTable("inbound_sync_logs");
            entity.HasKey(e => e.MutationId);
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ReceivedAt).HasColumnName("received_at");
        });
    }
}