using CurrencyRates.Mobile.Models;

namespace CurrencyRates.Mobile.Services;

/// <summary>
/// Interface for communicating with the Currency Rates API.
/// </summary>
public interface ICurrencyRateApiClient
{
    /// <summary>
    /// Fetches current currency rates from the API.
    /// </summary>
    /// <returns>A list of currency rate DTOs.</returns>
    Task<List<CurrencyRateDto>> GetRatesAsync();

    /// <summary>
    /// Fetches currency rates for a specific date.
    /// </summary>
    /// <param name="date">The date to fetch rates for.</param>
    /// <returns>A list of currency rate DTOs for the given date.</returns>
    Task<List<CurrencyRateDto>> GetRatesByDateAsync(DateTime date);

    /// <summary>
    /// Triggers a manual sync of currency rates on the server.
    /// </summary>
    Task SyncRatesAsync();
}
