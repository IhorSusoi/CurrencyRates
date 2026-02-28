using CurrencyRates.Domain.Entities;
using CurrencyRates.Domain.Enums;
using CurrencyRates.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CurrencyRates.Application.Services;

/// <summary>
/// Реалізує логіку отримання курсів валют.
/// Спочатку дивиться в БД, якщо даних немає або вони неповні —
/// іде в НБУ API за відсутніми валютами.
/// </summary>
public class CurrencyRateService : ICurrencyRateService
{
    // Список валют які ми підтримуємо — в одному місці щоб легко додати нову
    private static readonly string[] SupportedCurrencies = ["USD", "EUR", "DKK", "PLN"];

    private readonly ICurrencyRateRepository _repository;
    private readonly INbuApiClient _nbuApiClient;
    private readonly ILogger<CurrencyRateService> _logger;

    public CurrencyRateService(
        ICurrencyRateRepository repository,
        INbuApiClient nbuApiClient,
        ILogger<CurrencyRateService> logger)
    {
        _repository = repository;
        _nbuApiClient = nbuApiClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CurrencyRate>> GetOrFetchRatesAsync(DateOnly date, SourceType source)
    {
        // Дивимось які валюти вже є в БД на цю дату
        var existingCodes = await _repository.GetExistingCurrencyCodesAsync(date);

        // Знаходимо яких валют не вистачає
        var missingCodes = SupportedCurrencies
            .Except(existingCodes, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (missingCodes.Count > 0)
        {
            _logger.LogInformation(
                "Дата {Date}: в БД відсутні валюти {Currencies}. Запит до НБУ...",
                date, string.Join(", ", missingCodes));

            await FetchAndSaveRatesAsync(missingCodes, date, source);
        }

        // Повертаємо все що є в БД (і старе і щойно завантажене)
        return await _repository.GetByDateAsync(date);
    }

    /// <inheritdoc/>
    public async Task SyncTodayRatesAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        _logger.LogInformation("Початок автоматичної синхронізації на дату {Date}", today);

        var existingCodes = await _repository.GetExistingCurrencyCodesAsync(today);
        var missingCodes = SupportedCurrencies
            .Except(existingCodes, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (missingCodes.Count == 0)
        {
            _logger.LogInformation("Синхронізація {Date}: всі курси вже є в БД, пропускаємо", today);
            return;
        }

        await FetchAndSaveRatesAsync(missingCodes, today, SourceType.Auto);

        _logger.LogInformation("Автоматична синхронізація завершена на дату {Date}", today);
    }

    /// <summary>
    /// Запитує НБУ для кожної відсутньої валюти і зберігає результати.
    /// Якщо окрема валюта не повернулась — логуємо і продовжуємо з іншими.
    /// </summary>
    private async Task FetchAndSaveRatesAsync(
        IEnumerable<string> currencyCodes,
        DateOnly date,
        SourceType source)
    {
        var fetchedRates = new List<CurrencyRate>();

        foreach (var code in currencyCodes)
        {
            try
            {
                var rate = await _nbuApiClient.GetRateAsync(code, date);

                if (rate is not null)
                {
                    fetchedRates.Add(rate);
                    _logger.LogInformation("Отримано курс {Code} = {Rate} на {Date}", code, rate.Rate, date);
                }
                else
                {
                    _logger.LogWarning("НБУ не повернув курс для {Code} на дату {Date}", code, date);
                }
            }
            catch (Exception ex)
            {
                // Одна валюта не вийшла — не зупиняємо решту
                _logger.LogError(ex, "Помилка отримання курсу {Code} на дату {Date}", code, date);
            }
        }

        if (fetchedRates.Count > 0)
        {
            await _repository.SaveRatesAsync(fetchedRates, source);
        }
    }
}
