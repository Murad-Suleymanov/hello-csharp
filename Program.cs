using HelloCSharp;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom metrics service
builder.Services.AddSingleton<MetricsService>();
builder.Services.AddSingleton<WebSocketRegistry>();

// hello-websocket client
builder.Services.AddSingleton<HelloWebSocketLatest>();
builder.Services.AddHostedService<HelloWebSocketClient>();

// Downstream servisləri üçün named HttpClient-lər
var downstreamSection = builder.Configuration.GetSection("DownstreamServices");
builder.Services.AddHttpClient("HelloNodejs", client =>
{
    client.BaseAddress = new Uri(downstreamSection["HelloNodejs"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddHttpClient("HelloPython", client =>
{
    client.BaseAddress = new Uri(downstreamSection["HelloPython"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddScoped<DownstreamServicesClient>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseWebSockets();
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
