using System.Text.Json.Serialization;

namespace CurrencyRates.Infrastructure.ExternalServices;

/// <summary>
/// Відповідь НБУ API на запит курсу валюти.
/// Назви властивостей відповідають полям JSON від НБУ.
/// </summary>
internal class NbuRateDto
{
    /// <summary>Числовий код валюти.</summary>
    [JsonPropertyName("r030")]
    public int NumericCode { get; set; }

    /// <summary>Повна назва валюти.</summary>
    [JsonPropertyName("txt")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Курс відносно гривні.</summary>
    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    /// <summary>Буквений код валюти (USD, EUR...).</summary>
    [JsonPropertyName("cc")]
    public string Code { get; set; } = string.Empty;

    /// <summary>Дата на яку курс дійсний у форматі "dd.MM.yyyy".</summary>
    [JsonPropertyName("exchangedate")]
    public string ExchangeDate { get; set; } = string.Empty;
}
