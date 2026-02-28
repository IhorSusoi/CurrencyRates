namespace CurrencyRates.Application.Options;

/// <summary>
/// Налаштування модуля курсів валют.
/// Значення читаються з appsettings.json секції "CurrencySettings".
/// </summary>
public class CurrencyOptions
{
    /// <summary>Назва секції в appsettings.json.</summary>
    public const string SectionName = "CurrencySettings";

    /// <summary>
    /// Список валют які синхронізуються з НБУ.
    /// За замовчуванням: USD, EUR, DKK, PLN.
    /// </summary>
    public string[] SupportedCurrencies { get; set; } = ["USD", "EUR", "DKK", "PLN"];

    /// <summary>
    /// Час щоденної автоматичної синхронізації у форматі HH:mm.
    /// За замовчуванням: 16:00.
    /// </summary>
    public string DailySyncTime { get; set; } = "16:00";
}