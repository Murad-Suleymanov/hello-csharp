using Microsoft.AspNetCore.Mvc;

namespace HelloCSharp.Controllers;

[ApiController]
[Route("[controller]")]
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Həmişə 500 qaytarır - error-rate metrikaları və alert testləri üçün.
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogError("[/error] Test endpoint-i qəsdən 500 qaytarır.");

        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            source = "hello-csharp",
            @namespace = "hello-csharp",
            error = "Internal Server Error",
            message = "Bu endpoint test məqsədi ilə həmişə 500 qaytarır."
        });
    }
}
