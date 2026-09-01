using Microsoft.EntityFrameworkCore;
using Payhook.Api.Data.Configurations;
using Payhook.Api.Models;

namespace Payhook.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<RawEvent> RawEvents => Set<RawEvent>();

    public DbSet<ContractStatus> ContractStatuses => Set<ContractStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RawEventConfiguration());
        modelBuilder.ApplyConfiguration(new ContractStatusConfiguration());
    }
}
