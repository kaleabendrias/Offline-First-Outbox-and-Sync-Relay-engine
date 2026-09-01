using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncRelay.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNextRetryAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "next_retry_at",
                table: "outbox_events",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "next_retry_at",
                table: "outbox_events");
        }
    }
}
