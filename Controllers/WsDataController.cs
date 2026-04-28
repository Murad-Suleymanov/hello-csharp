using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HelloCSharp.Controllers;

[ApiController]
[Route("[controller]")]
public class WsDataController : ControllerBase
{
    private readonly HelloWebSocketLatest _store;

    public WsDataController(HelloWebSocketLatest store)
    {
        _store = store;
    }

    /// <summary>
    /// hello-websocket-dən alınan son datanı qaytarır.
    /// </summary>
    [HttpGet("latest")]
    public IActionResult Latest()
    {
        var raw = _store.Raw;
        if (raw is null)
            return Ok(new { source = "hello-csharp", upstream = "hello-websocket", data = (object?)"no data yet" });

        try
        {
            var parsed = JsonSerializer.Deserialize<JsonElement>(raw);
            return Ok(new { source = "hello-csharp", upstream = "hello-websocket", data = parsed });
        }
        catch
        {
            return Ok(new { source = "hello-csharp", upstream = "hello-websocket", data = (object?)raw });
        }
    }
}
