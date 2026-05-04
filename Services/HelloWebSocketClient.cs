using System.Net.WebSockets;
using System.Text;

namespace HelloCSharp;

public class HelloWebSocketLatest
{
    private volatile string? _raw;
    private volatile string? _sessionId;

    public string? Raw => _raw;
    public string? SessionId => _sessionId;

    public void UpdateData(string value) => _raw = value;
    public void SetSessionId(string id) => _sessionId = id;
    public void ClearSessionId() => _sessionId = null;
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
                var first = true;

                while (ws.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    var text = await ReceiveTextAsync(ws, buffer, stoppingToken);
                    if (text is null) break;

                    if (first)
                    {
                        _store.SetSessionId(text);
                        _logger.LogInformation("hello-websocket session ID alındı: {Id}", text);
                        first = false;
                    }
                    else
                    {
                        _store.UpdateData(text);
                    }
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "hello-websocket bağlantısı kəsildi, 5 san sonra yenidən cəhd ediləcək");
                await Task.Delay(5000, stoppingToken);
            }
            finally
            {
                _store.ClearSessionId();
            }
        }
    }

    private static async Task<string?> ReceiveTextAsync(
        ClientWebSocket ws, byte[] buffer, CancellationToken ct)
    {
        var ms = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await ws.ReceiveAsync(buffer, ct);
            if (result.MessageType == WebSocketMessageType.Close) return null;
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
    }
}
