using Microsoft.AspNetCore.Mvc;

namespace HelloCSharp.Controllers;

[ApiController]
[Route("[controller]")]
public class AggregateController : ControllerBase
{
    private readonly DownstreamServicesClient _downstream;

    public AggregateController(DownstreamServicesClient downstream)
    {
        _downstream = downstream;
    }

    /// <summary>
    /// hello-nodejs və hello-python servislərinə müraciət edib cavabları birləşdirir.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string name = "World")
    {
        var nodejsTask = _downstream.GetNodejsHelloAsync(name);
        var pythonTask = _downstream.GetPythonRootAsync();
        await Task.WhenAll(nodejsTask, pythonTask);
        var nodejsHello = nodejsTask.Result;
        var pythonRoot = pythonTask.Result;

        return Ok(new
        {
            source = "hello-csharp",
            @namespace = "hello-csharp",
            calledServices = new[]
            {
                new
                {
                    service = "hello-nodejs",
                    @namespace = "hello-nodejs",
                    response = (object?)nodejsHello ?? "unreachable"
                },
                new
                {
                    service = "hello-python",
                    @namespace = "hello-python",
                    response = (object?)pythonRoot ?? "unreachable"
                }
            }
        });
    }

    /// <summary>
    /// hello-csharp → hello-nodejs → hello-python ardıcıl zənciri.
    /// Istio ambient mode-da üç node arasında iki ayrı edge göstərir.
    /// </summary>
    [HttpGet("chain")]
    public async Task<IActionResult> Chain()
    {
        var chain = await _downstream.GetNodejsChainAsync();
        return Ok(new
        {
            source = "hello-csharp",
            calledService = new
            {
                service = "hello-nodejs",
                response = (object?)chain ?? "unreachable"
            }
        });
    }

    /// <summary>
    /// Bütün servislərin health statusunu yoxlayır.
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        var nodejsTask = _downstream.GetNodejsHealthAsync();
        var pythonTask = _downstream.GetPythonHealthAsync();
        await Task.WhenAll(nodejsTask, pythonTask);
        var nodejsHealth = nodejsTask.Result;
        var pythonHealth = pythonTask.Result;

        return Ok(new
        {
            source = "hello-csharp",
            status = "healthy",
            downstreamHealth = new[]
            {
                new
                {
                    service = "hello-nodejs",
                    status = nodejsHealth.HasValue ? "reachable" : "unreachable",
                    response = (object?)nodejsHealth ?? "unreachable"
                },
                new
                {
                    service = "hello-python",
                    status = pythonHealth.HasValue ? "reachable" : "unreachable",
                    response = (object?)pythonHealth ?? "unreachable"
                }
            }
        });
    }
}
