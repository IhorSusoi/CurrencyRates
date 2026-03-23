namespace CurrencyRates.Mobile.Models;

/// <summary>
/// Data transfer object representing a currency exchange rate from the API.
/// </summary>
public class CurrencyRateDto
{
    /// <summary>
    /// ISO currency code (e.g., USD, EUR).
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the currency.
    /// </summary>
    public string CurrencyName { get; set; } = string.Empty;

    /// <summary>
    /// Exchange rate relative to UAH.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Date of the exchange rate.
    /// </summary>
    public DateTime RateDate { get; set; }

    /// <summary>
    /// Source of the rate: "Auto" (scheduled) or "Manual" (user-triggered).
    /// </summary>
    public string Source { get; set; } = string.Empty;
}
