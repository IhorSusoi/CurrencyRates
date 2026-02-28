using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyRates.Domain.Entities;

namespace CurrencyRates.Domain.Interfaces;

/// <summary>
/// Інтерфейс для роботи з API Національного банку України (НБУ) для отримання актуальних курсів валют.
/// </summary>
public interface INbuApiClient
{
    /// <summary>
    /// Отримує курс однієї валюти на вказану дату з НБУ API.
    /// </summary>
    /// <param name="currencyCode">Код валюти (USD, EUR, DKK, PLN).</param>
    /// <param name="date">Дата на яку потрібен курс.</param>
    /// <returns>
    /// Курс валюти або <c>null</c> якщо НБУ не повернув дані
    /// (наприклад помилка мережі).
    /// </returns>
    Task<CurrencyRate?> GetRateAsync(string currencyCode, DateOnly date);
}
