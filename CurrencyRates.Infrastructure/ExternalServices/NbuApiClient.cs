using System.Text.Json;
using CurrencyRates.Domain.Entities;
using CurrencyRates.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CurrencyRates.Infrastructure.ExternalServices;

/// <summary>
/// Клієнт для отримання курсів валют з API Національного банку України.
/// Endpoint: https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange
/// </summary>
public class NbuApiClient : INbuApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NbuApiClient> _logger;

    public NbuApiClient(HttpClient httpClient, ILogger<NbuApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<CurrencyRate?> GetRateAsync(string currencyCode, DateOnly date)
    {
        var dateString = date.ToString("yyyyMMdd");
        var url = $"?valcode={currencyCode}&date={dateString}&json";

        _logger.LogInformation("Запит до НБУ: {Url}", url);

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "НБУ повернув статус {Status} для {Code} на {Date}",
                response.StatusCode, currencyCode, date);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var rates = JsonSerializer.Deserialize<List<NbuRateDto>>(content);

        if(rates is null || rates.Count == 0)
        {
            _logger.LogWarning(
                "НБУ не повернув дані для {Code} на {Date}",
                currencyCode, date);
            return null;
        }

        var nbuRate = rates[0];

        return new CurrencyRate
        {
            Rate = nbuRate.Rate,
            RateDate = date,
            CreatedAt = DateTime.UtcNow
        };
    }
}
