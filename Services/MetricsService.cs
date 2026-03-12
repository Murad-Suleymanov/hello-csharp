using Prometheus;

namespace HelloCSharp;

/// <summary>
/// Custom application metrics for Prometheus
/// </summary>
public class MetricsService
{
    // Counter: total number of weather API requests
    private readonly Counter _weatherRequestsTotal = Metrics.CreateCounter(
        "hellocsharp_weather_requests_total",
        "Total number of weather forecast requests");

    // Histogram: request processing duration
    private readonly Histogram _requestDuration = Metrics.CreateHistogram(
        "hellocsharp_request_duration_seconds",
        "Duration of weather API requests in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
        });

    // Gauge: active requests (simulated)
    private readonly Gauge _activeRequests = Metrics.CreateGauge(
        "hellocsharp_active_requests",
        "Number of currently processing requests");

    public void RecordWeatherRequest()
    {
        _weatherRequestsTotal.Inc();
    }

    public IDisposable MeasureRequestDuration()
    {
        _activeRequests.Inc();
        return new RequestTracker(_activeRequests, _requestDuration);
    }

    private class RequestTracker : IDisposable
    {
        private readonly Gauge _activeRequests;
        private readonly Histogram.Timer _timer;

        public RequestTracker(Gauge activeRequests, Histogram histogram)
        {
            _activeRequests = activeRequests;
            _timer = histogram.NewTimer();
        }

        public void Dispose()
        {
            _timer.Dispose();
            _activeRequests.Dec();
        }
    }
}
