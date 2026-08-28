using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AegiFinance.Web.Middleware;

public sealed class RequestTelemetryMiddleware
{
    private static readonly ActivitySource ActivitySource = new("AegiFinance.Web");
    private static readonly Meter Meter = new("AegiFinance.Web");
    private static readonly Counter<long> Requests = Meter.CreateCounter<long>("aegifinance.http.requests");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("aegifinance.http.duration", "ms");
    private readonly RequestDelegate _next;
    public RequestTelemetryMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ILogger<RequestTelemetryMiddleware> logger)
    {
        var started = Stopwatch.GetTimestamp();
        using var activity = ActivitySource.StartActivity($"{context.Request.Method} {context.Request.Path}", ActivityKind.Server);
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        context.Response.Headers["X-Trace-Id"] = traceId;
        try { await _next(context); }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            var tags = new TagList { { "http.method", context.Request.Method }, { "http.status_code", context.Response.StatusCode } };
            Requests.Add(1, tags); Duration.Record(elapsed, tags);
            activity?.SetTag("http.response.status_code", context.Response.StatusCode);
            logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {Elapsed:0.0} ms trace={TraceId}", context.Request.Method, context.Request.Path, context.Response.StatusCode, elapsed, traceId);
        }
    }
}
