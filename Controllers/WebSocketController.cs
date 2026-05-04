using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace HelloCSharp.Controllers;

[ApiController]
[Route("[controller]")]
public class WebSocketController : ControllerBase
{
    private readonly WebSocketRegistry _registry;
    private readonly HelloWebSocketLatest _store;

    public WebSocketController(WebSocketRegistry registry, HelloWebSocketLatest store)
    {
        _registry = registry;
        _store = store;
    }

    [HttpGet("stream")]
    public async Task Stream([FromQuery] int interval = 1000)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsync("WebSocket request gözlənilir.");
            return;
        }

        using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var ct = HttpContext.RequestAborted;

        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(interval));
            while (await timer.WaitForNextTickAsync(ct) && ws.State == WebSocketState.Open)
            {
                var payload = _store.Raw ?? "{\"data\":\"no data yet\"}";
                var bytes = Encoding.UTF8.GetBytes(payload);
                await ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
            }
        }
        catch (OperationCanceledException) { }

        if (ws.State == WebSocketState.Open)
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
    }

    [HttpGet]
    public async Task Get([FromQuery] int interval = 1000)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsync("WebSocket request gözlənilir.");
            return;
        }

        using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();

        var id = _registry.Register(out var session);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(session.Cts.Token, HttpContext.RequestAborted);
        var ct = linked.Token;

        var idBytes = Encoding.UTF8.GetBytes(id);
        await ws.SendAsync(idBytes, WebSocketMessageType.Text, endOfMessage: true, ct);

        // receive loop
        var receiveTask = Task.Run(async () =>
        {
            var buffer = new byte[4096];
            while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await ws.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close) break;
            }
        }, ct);

        // send loop — hər `interval` ms-də bir özü mesaj göndərir
        var sendTask = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(interval));
            while (await timer.WaitForNextTickAsync(ct) && ws.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString("O"));
                await ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
            }
        }, ct);

        try
        {
            await Task.WhenAny(receiveTask, sendTask);
        }
        catch (OperationCanceledException) { }
        finally
        {
            _registry.Remove(id);
        }

        if (ws.State == WebSocketState.Open)
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
    }

    [HttpGet("active")]
    public IActionResult Active() => Ok(_registry.ActiveIds);

    [HttpPost("{id}/send")]
    public IActionResult Send(string id, [FromBody] string message)
    {
        if (!_registry.Send(id, message))
            return NotFound(new { error = "Session tapılmadı" });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Close(string id)
    {
        if (!_registry.Cancel(id))
            return NotFound(new { error = "Session tapılmadı" });

        return NoContent();
    }
}
