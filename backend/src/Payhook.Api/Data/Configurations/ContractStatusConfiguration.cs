using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payhook.Api.Models;

namespace Payhook.Api.Data.Configurations;

public sealed class ContractStatusConfiguration : IEntityTypeConfiguration<ContractStatus>
{
    public void Configure(EntityTypeBuilder<ContractStatus> builder)
    {
        builder.ToTable("contract_statuses");

        builder.HasKey(contractStatus => contractStatus.ContractId)
            .HasName("pk_contract_statuses");

        builder.Property(contractStatus => contractStatus.ContractId)
            .HasColumnName("contract_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(contractStatus => contractStatus.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(contractStatus => contractStatus.Amount)
            .HasColumnName("amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(contractStatus => contractStatus.PaymentDate)
            .HasColumnName("payment_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(contractStatus => contractStatus.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
