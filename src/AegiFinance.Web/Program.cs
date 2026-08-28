using AegiFinance.Application;
using AegiFinance.Infrastructure;
using AegiFinance.Web.Middleware;
using AegiFinance.Web.Seed;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

const string NextJsCorsPolicy = "NextJsClient";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(NextJsCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add services
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { service = "AegiFinance.Web", status = "running" })).AllowAnonymous();

app.UseExceptionHandler();

app.UseForwardedHeaders();
app.UseMiddleware<RequestTelemetryMiddleware>();
app.UseHttpsRedirection();
app.UseCors(NextJsCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health/live", () => Results.Ok(new { status = "ok", service = "AegiFinance.Web", utc = DateTime.UtcNow })).AllowAnonymous();

app.MapGet("/health/ready", async (AegiFinance.Infrastructure.Data.ApplicationDbContext context, CancellationToken cancellationToken) =>
    await context.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "ok", service = "AegiFinance.Web", database = "connected", utc = DateTime.UtcNow })
        : Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Database unavailable"))
    .AllowAnonymous();

app.MapGet("/health", () => Results.Redirect("/health/ready", permanent: false)).AllowAnonymous();

app.MapControllers();

// Every authorization policy declared by an endpoint becomes a manageable
// permission automatically. This prevents policy codes from drifting away
// from the role-permission catalog.
var discoveredPermissionCodes = ((IEndpointRouteBuilder)app).DataSources
    .SelectMany(source => source.Endpoints)
    .SelectMany(endpoint => endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>())
    .Select(metadata => metadata.Policy)
    .Where(policy => !string.IsNullOrWhiteSpace(policy))
    .Cast<string>()
    .Distinct(StringComparer.Ordinal)
    .ToArray();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AegiFinance.Infrastructure.Data.ApplicationDbContext>();
    Log.Information("Applying database migrations...");
    await context.Database.MigrateAsync();
    Log.Information("Database migrations applied successfully.");

    await SeedData.InitializeAsync(scope.ServiceProvider, discoveredPermissionCodes);
}

app.Run();
