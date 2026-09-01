using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payhook.Api.Models;

namespace Payhook.Api.Data.Configurations;

public sealed class RawEventConfiguration : IEntityTypeConfiguration<RawEvent>
{
    public void Configure(EntityTypeBuilder<RawEvent> builder)
    {
        builder.ToTable("raw_events");

        builder.HasKey(rawEvent => rawEvent.Id)
            .HasName("pk_raw_events");

        builder.Property(rawEvent => rawEvent.Id)
            .HasColumnName("id");

        builder.Property(rawEvent => rawEvent.TransactionId)
            .HasColumnName("transaction_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(rawEvent => rawEvent.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(rawEvent => rawEvent.ReceivedAt)
            .HasColumnName("received_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(rawEvent => rawEvent.ProcessingStatus)
            .HasColumnName("processing_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(rawEvent => rawEvent.ProcessingError)
            .HasColumnName("processing_error")
            .HasMaxLength(1000);

        builder.HasIndex(rawEvent => rawEvent.TransactionId)
            .IsUnique()
            .HasDatabaseName("ix_raw_events_transaction_id");
    }
}
