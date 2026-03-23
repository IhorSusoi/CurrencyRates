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
    public Dictionary<string, string> SupportedCurrencies { get; set; } = new()
    {
    { "USD", "Долар США" },
    { "EUR", "Євро" },
    { "DKK", "Данська крона" },
    { "PLN", "Польський злотий" }
    };

    /// <summary>
    /// Час щоденної автоматичної синхронізації у форматі HH:mm.
    /// За замовчуванням: 16:00.
    /// </summary>
    public string DailySyncTime { get; set; } = "16:00";

    /// <summary>Базова адреса API Національного банку України.</summary>
    public string NbuApiBaseUrl { get; set; } = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange";

    /// <summary>
    /// Кількість повторних спроб синхронізації за 24 години,
    /// якщо НБУ не повернув курси на вказану дату.
    /// За замовчуванням: 3.
    /// </summary>
    public int SyncRetryCount { get; set; } = 3;
}