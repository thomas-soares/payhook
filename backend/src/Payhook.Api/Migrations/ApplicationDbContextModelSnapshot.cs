using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Payhook.Api.Data;
using Payhook.Api.Models;

#nullable disable

namespace Payhook.Api.Migrations;

[DbContext(typeof(ApplicationDbContext))]
partial class ApplicationDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("Payhook.Api.Models.ContractStatus", builder =>
        {
            builder.Property<string>("ContractId")
                .HasMaxLength(100)
                .HasColumnType("character varying(100)")
                .HasColumnName("contract_id");

            builder.Property<decimal>("Amount")
                .HasColumnType("numeric(18,2)")
                .HasColumnName("amount");

            builder.Property<DateTimeOffset>("PaymentDate")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("payment_date");

            builder.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("character varying(50)")
                .HasColumnName("status");

            builder.Property<DateTimeOffset>("UpdatedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            builder.HasKey("ContractId")
                .HasName("pk_contract_statuses");

            builder.ToTable("contract_statuses");
        });

        modelBuilder.Entity("Payhook.Api.Models.RawEvent", builder =>
        {
            builder.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                .HasColumnName("id");

            builder.Property<string>("PayloadJson")
                .IsRequired()
                .HasColumnType("jsonb")
                .HasColumnName("payload_json");

            builder.Property<ProcessingStatus>("ProcessingStatus")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasColumnName("processing_status");

            builder.Property<DateTimeOffset>("ReceivedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .HasColumnName("received_at");

            builder.Property<string>("TransactionId")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)")
                .HasColumnName("transaction_id");

            builder.HasKey("Id")
                .HasName("pk_raw_events");

            builder.HasIndex("TransactionId")
                .IsUnique()
                .HasDatabaseName("ix_raw_events_transaction_id");

            builder.ToTable("raw_events");
        });
#pragma warning restore 612, 618
    }
}
