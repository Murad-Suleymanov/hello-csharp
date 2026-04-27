using System.Text.Json;

namespace HelloCSharp;

public class DownstreamServicesClient
{
    private readonly HttpClient _nodejsClient;
    private readonly HttpClient _pythonClient;
    private readonly ILogger<DownstreamServicesClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DownstreamServicesClient(
        IHttpClientFactory factory,
        ILogger<DownstreamServicesClient> logger)
    {
        _nodejsClient = factory.CreateClient("HelloNodejs");
        _pythonClient = factory.CreateClient("HelloPython");
        _logger = logger;
    }

    public async Task<JsonElement?> GetNodejsHelloAsync(string name = "World")
    {
        try
        {
            var response = await _nodejsClient.GetAsync($"/api/hello?name={Uri.EscapeDataString(name)}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "hello-nodejs servisinə müraciət uğursuz oldu");
            return null;
        }
    }

    public async Task<JsonElement?> GetPythonRootAsync()
    {
        try
        {
            var response = await _pythonClient.GetAsync("/");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "hello-python servisinə müraciət uğursuz oldu");
            return null;
        }
    }

    public async Task<JsonElement?> GetNodejsHealthAsync()
    {
        try
        {
            var response = await _nodejsClient.GetAsync("/health");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "hello-nodejs health yoxlaması uğursuz oldu");
            return null;
        }
    }

    public async Task<JsonElement?> GetPythonHealthAsync()
    {
        try
        {
            var response = await _pythonClient.GetAsync("/health");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "hello-python health yoxlaması uğursuz oldu");
            return null;
        }
    }
}
