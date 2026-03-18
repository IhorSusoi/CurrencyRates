using System.Net.Http.Json;
using CurrencyRates.Mobile.Models;

namespace CurrencyRates.Mobile.Services;

/// <summary>
/// HTTP client for communicating with the Currency Rates API.
/// Uses HttpClient injected via DI with a named/typed configuration.
/// </summary>
public class CurrencyRateApiClient : ICurrencyRateApiClient
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="CurrencyRateApiClient"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured with the API base address.</param>
    public CurrencyRateApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<List<CurrencyRateDto>> GetRatesAsync()
    {
        var dateString = DateTime.Today.ToString("yyyy-M-d");
        var rates = await this.httpClient.GetFromJsonAsync<List<CurrencyRateDto>>($"api/currency-rates?date={dateString}");
        return rates ?? new List<CurrencyRateDto>();
    }

    /// <inheritdoc />
    public async Task<List<CurrencyRateDto>> GetRatesByDateAsync(DateTime date)
    {
        var dateString = date.ToString("yyyy-M-d");
        var rates = await this.httpClient.GetFromJsonAsync<List<CurrencyRateDto>>($"api/currency-rates?date={dateString}");
        return rates ?? new List<CurrencyRateDto>();
    }

    /// <inheritdoc />
    public async Task SyncRatesAsync()
    {
        var response = await this.httpClient.PostAsync("api/currency-rates/sync", null);
        response.EnsureSuccessStatusCode();
    }
}
