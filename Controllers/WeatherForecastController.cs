using Microsoft.AspNetCore.Mvc;
using HelloCSharp;

namespace HelloCSharp.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly MetricsService _metricsService;

    public WeatherForecastController(MetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        using (_metricsService.MeasureRequestDuration())
        {
            _metricsService.RecordWeatherRequest();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
        }
    }
}
