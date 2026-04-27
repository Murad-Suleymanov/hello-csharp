using Microsoft.AspNetCore.Mvc;

namespace HelloCSharp.Controllers;

[ApiController]
[Route("[controller]")]
public class DelayController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int ms = 0)
    {
        if (ms < 0 || ms > 30_000)
            return BadRequest(new { error = "ms must be between 0 and 30000" });

        await Task.Delay(ms);

        return NoContent();
    }
}
