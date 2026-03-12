using HelloCSharp;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom metrics service
builder.Services.AddSingleton<MetricsService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Prometheus metrics endpoint - same port as the app (default: /metrics)
// prometheus-net adds built-in HTTP metrics automatically
app.UseHttpMetrics();
app.MapMetrics();

app.Run();
