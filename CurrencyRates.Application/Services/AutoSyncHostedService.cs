using CurrencyRates.Application.Options;
using CurrencyRates.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace CurrencyRates.Application.Services;

/// <summary>
/// Фонова служба яка автоматично синхронізує курси валют щодня о 16:00.
/// Також робить перевірку при старті застосунку.
/// </summary>
public class AutoSyncHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoSyncHostedService> _logger;
    private readonly TimeOnly _syncTime;

    public AutoSyncHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<AutoSyncHostedService> logger,
        IOptions<CurrencyOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _syncTime = TimeOnly.Parse(options.Value.DailySyncTime, CultureInfo.InvariantCulture);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunStartupSyncAsync();

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

    private async Task RunStartupSyncAsync()
    {
        _logger.LogInformation("Перевірка курсів при старті застосунку...");
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ICurrencyRateService>();
            await service.SyncTodayRatesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка синхронізації при старті");
        }
    }

    private async Task RunDailySyncAsync()
    {
        _logger.LogInformation("Початок планової щоденної синхронізації");
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ICurrencyRateService>();
            await service.SyncTodayRatesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка щоденної синхронізації");
        }
    }

    private TimeSpan CalculateDelayUntilNextSync()
    {
        var now = DateTime.Now;
        var todaySync = DateTime.Today.Add(_syncTime.ToTimeSpan());
        var nextSync = now < todaySync ? todaySync : todaySync.AddDays(1);
        return nextSync - now;
    }
}