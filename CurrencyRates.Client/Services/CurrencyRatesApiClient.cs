using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CurrencyRates.Client.Services;

/// <summary>
/// HTTP клієнт для отримання курсів валют з API.
/// </summary>
public class CurrencyRatesApiClient
{
    private readonly HttpClient _httpClient;

    public CurrencyRatesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Отримує курси валют на вказану дату.
    /// </summary>
    public async Task<(List<CurrencyRateModel>? Rates, string? Error)> GetRatesAsync(DateOnly date)
    {
        try
        {
            var url = $"api/currency-rates?date={date:yyyy-MM-dd}";
            var rates = await _httpClient.GetFromJsonAsync<List<CurrencyRateModel>>(url);
            return (rates, null);
        }
        catch (HttpRequestException ex) when ((int?)ex.StatusCode == 503)
        {
            return (null, "НБУ API наразі недоступний. Спробуйте пізніше.");
        }
        catch (Exception ex)
        {
            return (null, $"Помилка: {ex.Message}");
        }
    }
}

/// <summary>
/// Модель курсу валюти для відображення в UI.
/// </summary>
public class CurrencyRateModel
{
    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = string.Empty;

    [JsonPropertyName("currencyName")]
    public string CurrencyName { get; set; } = string.Empty;

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    [JsonPropertyName("rateDate")]
    public DateOnly RateDate { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}