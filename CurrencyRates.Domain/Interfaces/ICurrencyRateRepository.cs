using CurrencyRates.Domain.Entities;
using CurrencyRates.Domain.Enums;

namespace CurrencyRates.Domain.Interfaces;

/// <summary>
/// Інтерфейс для роботи з курсами валют в базі даних.
/// </summary>
public interface ICurrencyRateRepository
{
    /// <summary>
    /// Повертає курси валют на вказану дату.
    /// </summary>
    /// <param name="date">Дата курсу.</param>
    /// <returns>Список курсів. Може бути порожнім якщо даних немає.</returns>
    Task<IReadOnlyList<CurrencyRate>> GetByDateAsync(DateOnly date);

    /// <summary>
    /// Повертає коди валют для яких вже є курс на вказану дату.
    /// Використовується щоб не дублювати записи.
    /// </summary>
    /// <param name="date">Дата курсу.</param>
    Task<IReadOnlyList<string>> GetExistingCurrencyCodesAsync(DateOnly date);

    /// <summary>
    /// Зберігає курси валют в базу даних.
    /// Якщо курс для цієї валюти на цю дату вже є — пропускає (не дублює).
    /// </summary>
    /// <param name="rates">Список курсів для збереження.</param>
    /// <param name="source">Звідки прийшли дані: Auto або Manual.</param>
    Task SaveRatesAsync(IEnumerable<CurrencyRate> rates, SourceType source);

    /// <summary>
    /// Повертає ID валюти з довідника по її коду.
    /// </summary>
    /// <param name="code">Код валюти (USD, EUR...).</param>
    /// <returns>ID або null якщо валюта не знайдена в довіднику.</returns>
    Task<int?> GetCurrencyIdByCodeAsync(string code);

    /// <summary>
    /// Додає валюту в довідник якщо її ще немає.
    /// </summary>
    Task EnsureCurrencyExistsAsync(string code, string name);
}
