using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Payhook.Api.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Default database connection string is not configured.");

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});
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
