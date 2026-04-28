using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace HelloCSharp;

public class HelloWebSocketLatest
{
    private volatile string? _raw;
    public string? Raw => _raw;
    public void Update(string value) => _raw = value;
}

public class HelloWebSocketClient : BackgroundService
{
    private readonly string _wsUrl;
    private readonly HelloWebSocketLatest _store;
    private readonly ILogger<HelloWebSocketClient> _logger;

    public HelloWebSocketClient(
        IConfiguration config,
        HelloWebSocketLatest store,
        ILogger<HelloWebSocketClient> logger)
    {
        var base_ = config["DownstreamServices:HelloWebsocket"]!;
        _wsUrl = base_.Replace("http://", "ws://").Replace("https://", "wss://") + "/ws";
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri(_wsUrl), stoppingToken);
                _logger.LogInformation("hello-websocket-ə qoşuldu: {Url}", _wsUrl);

                var buffer = new byte[4096];
                while (ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    var result = await ws.ReceiveAsync(buffer, stoppingToken);
                    if (result.MessageType == WebSocketMessageType.Close) break;
                    _store.Update(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "hello-websocket bağlantısı kəsildi, 5 san sonra yenidən cəhd ediləcək");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
