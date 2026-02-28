using CurrencyRates.Application.Options;
using CurrencyRates.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CurrencyRates.Application.Services;

/// <summary>
/// Фонова служба яка автоматично синхронізує курси валют щодня о 16:00.
/// Також робить перевірку при старті застосунку.
/// </summary>
/// <remarks>
/// НБУ публікує курси приблизно о 15:00–15:30 щодня.
/// Ми робимо запит о 16:00 щоб бути впевнені що дані вже опубліковані.
/// </remarks>
public class AutoSyncHostedService : BackgroundService
{
    private readonly ICurrencyRateService _currencyRateService;
    private readonly ILogger<AutoSyncHostedService> _logger;
    private readonly TimeOnly _syncTime;

    // О котрій годині робити синхронізацію щодня
    private static readonly TimeOnly SyncTime = new(16, 00);

    public AutoSyncHostedService(
        ICurrencyRateService currencyRateService,
        ILogger<AutoSyncHostedService> logger,
        IOptions<CurrencyOptions> options)
    {
        _currencyRateService = currencyRateService;
        _logger = logger;
        _syncTime = TimeOnly.Parse(options.Value.DailySyncTime);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Перевірка при старті — якщо курсів на сьогодні немає, завантажуємо
        await RunStartupSyncAsync();

        // Далі чекаємо потрібного часу і синхронізуємо щодня
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextSync();

            _logger.LogInformation(
                "Наступна синхронізація курсів через {Hours}г {Minutes}хв",
                (int)delay.TotalHours, delay.Minutes);

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            await RunDailySyncAsync();
        }
    }

    /// <summary>
    /// Синхронізація при старті застосунку.
    /// </summary>
    private async Task RunStartupSyncAsync()
    {
        _logger.LogInformation("Перевірка курсів при старті застосунку...");
        try
        {
            await _currencyRateService.SyncTodayRatesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка синхронізації при старті");
        }
    }

    /// <summary>
    /// Щоденна синхронізація о 16:00.
    /// </summary>
    private async Task RunDailySyncAsync()
    {
        _logger.LogInformation("Початок планової щоденної синхронізації");
        try
        {
            await _currencyRateService.SyncTodayRatesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка щоденної синхронізації");
        }
    }

    /// <summary>
    /// Розраховує скільки часу чекати до наступного запуску о 16:00.
    /// </summary>
    private static TimeSpan CalculateDelayUntilNextSync()
    {
        var now = DateTime.Now;
        var todaySync = DateTime.Today.Add(_syncTime.ToTimeSpan());

        // Якщо 16:00 сьогодні вже минуло — плануємо на завтра
        var nextSync = now < todaySync ? todaySync : todaySync.AddDays(1);

        return nextSync - now;
    }
}