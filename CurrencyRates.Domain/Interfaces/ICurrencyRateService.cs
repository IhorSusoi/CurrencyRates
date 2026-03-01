using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyRates.Domain.Entities;
using CurrencyRates.Domain.Enums;

namespace CurrencyRates.Domain.Interfaces;

/// <summary>
/// Інтерфейс для сервісу, який відповідає за логіку роботи з курсами валют, включаючи отримання, збереження та обробку даних про курси валют.
/// </summary>
public interface ICurrencyRateService
{
    /// <summary>
    /// Повертає курси валют на вказану дату.
    /// Якщо даних в БД немає або вони неповні — автоматично
    /// завантажує відсутні курси з НБУ і зберігає їх.
    /// </summary>
    /// <param name="date">Дата курсу.</param>
    /// <param name="source">З яким позначенням зберегти нові дані.</param>
    Task<IReadOnlyList<CurrencyRate>> GetOrFetchRatesAsync(DateOnly date, SourceType source);

    /// <summary>
    /// Синхронізує курси на завтрішню дату.
    /// Викликається автоматично по розкладу.
    /// </summary>
    Task SyncRatesAsync(DateOnly date);
}
