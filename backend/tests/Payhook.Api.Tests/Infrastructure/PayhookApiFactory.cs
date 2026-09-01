using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Payhook.Api.Data;

namespace Payhook.Api.Tests.Infrastructure;

public sealed class PayhookApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WebhookSecurity:SignatureSecret"] = "test-secret",
                ["PaymentProcessing:ProcessingDelay"] = "00:00:00",
                ["PaymentProcessing:QueueCapacity"] = "1000",
                ["PaymentProcessing:PendingScanInterval"] = "00:00:01",
                ["PaymentProcessing:PendingBatchSize"] = "50",
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=payhook;Username=payhook;Password=payhook"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });
        });
    }
}
