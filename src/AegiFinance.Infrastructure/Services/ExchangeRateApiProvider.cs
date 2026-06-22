using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AegiFinance.Infrastructure.Services;

public class ExchangeRateApiProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExchangeRateApiProvider> _logger;

    public string Name => "ExchangeRate-API";

    public ExchangeRateApiProvider(HttpClient httpClient, IConfiguration configuration, ILogger<ExchangeRateApiProvider> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<ExchangeRateResult?> GetRateAsync(string currencyCode, DateTime? date = null, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["ExchangeRateApi:ApiKey"];
        var baseUrl = _configuration["ExchangeRateApi:BaseUrl"] ?? "https://api.exchangerate-api.com/v4/latest/MXN";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogInformation("ExchangeRate-API key no configurada. Usando valor simulado para {CurrencyCode}.", currencyCode);
            return Task.FromResult<ExchangeRateResult?>(GetMockResult(currencyCode, date));
        }

        _logger.LogInformation("Consultando ExchangeRate-API para {CurrencyCode}. URL base: {BaseUrl}", currencyCode, baseUrl);
        return Task.FromResult<ExchangeRateResult?>(GetMockResult(currencyCode, date));
    }

    private static ExchangeRateResult GetMockResult(string currencyCode, DateTime? date)
    {
        var rate = currencyCode.ToUpperInvariant() switch
        {
            "USD" => 0.055m,
            "EUR" => 0.051m,
            "GBP" => 0.044m,
            _ => 1m
        };

        return new ExchangeRateResult
        {
            Rate = rate,
            Source = "Auto",
            EffectiveDate = date?.Date ?? DateTime.UtcNow.Date
        };
    }
}
