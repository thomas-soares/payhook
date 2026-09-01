using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Payhook.Api.Data;
using Payhook.Api.Options;
using Payhook.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Default database connection string is not configured.");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});
builder.Services.AddOptions<WebhookSecurityOptions>()
    .Bind(builder.Configuration.GetSection(WebhookSecurityOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<PaymentProcessingOptions>()
    .Bind(builder.Configuration.GetSection(PaymentProcessingOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<IPaymentProcessingQueue, PaymentProcessingQueue>();
builder.Services.AddScoped<PaymentWebhookService>();
builder.Services.AddScoped<PaymentEventProcessor>();
builder.Services.AddScoped<PaymentQueryService>();
builder.Services.AddSingleton<WebhookSecurityService>();
builder.Services.AddHostedService<PaymentProcessingWorker>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payhook API",
        Version = "v1",
        Description = "Payment webhook service API"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Payhook API v1");
    });
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "payhook-api"
}));

app.MapControllers();

app.Run();

public partial class Program;
