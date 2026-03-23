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
/// Також робить перевірку при старті застосунку (сьогодні + завтра).
/// Якщо НБУ не повернув курси — повторює спроби (за замовчуванням 3 рази за 24 години).
/// </summary>
public class AutoSyncHostedService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<AutoSyncHostedService> logger;
    private readonly TimeOnly syncTime;
    private readonly int retryCount;

    public AutoSyncHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<AutoSyncHostedService> logger,
        IOptions<CurrencyOptions> options)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.syncTime = TimeOnly.Parse(options.Value.DailySyncTime, CultureInfo.InvariantCulture);
        this.retryCount = options.Value.SyncRetryCount;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await this.RunStartupSyncAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = this.CalculateDelayUntilNextSync();

            this.logger.LogInformation(
                "Наступна синхронізація курсів через {Hours}г {Minutes}хв",
                (int)delay.TotalHours, delay.Minutes);

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            await this.RunDailySyncAsync(stoppingToken);
        }
    }

    /// <summary>
    /// Синхронізує курси при старті: сьогоднішня дата + завтрашня дата.
    /// Для кожної дати виконує повторні спроби якщо НБУ не повернув курси.
    /// </summary>
    private async Task RunStartupSyncAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Перевірка курсів при старті застосунку...");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);

        await this.SyncWithRetryAsync(today, stoppingToken);
        await this.SyncWithRetryAsync(tomorrow, stoppingToken);
    }

    /// <summary>
    /// Планова щоденна синхронізація на завтрашню дату з повторними спробами.
    /// </summary>
    private async Task RunDailySyncAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Початок планової щоденної синхронізації");

        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        await this.SyncWithRetryAsync(tomorrow, stoppingToken);
    }

    /// <summary>
    /// Синхронізує курси на вказану дату з повторними спробами.
    /// Якщо НБУ не повернув всі курси — повторює до <see cref="retryCount"/> разів
    /// з рівномірним інтервалом протягом 24 годин.
    /// </summary>
    private async Task SyncWithRetryAsync(DateOnly date, CancellationToken stoppingToken)
    {
        var retryInterval = TimeSpan.FromHours(24.0 / this.retryCount);

        for (int attempt = 1; attempt <= this.retryCount; attempt++)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                this.logger.LogInformation(
                    "Синхронізація {Date}: спроба {Attempt} з {Total}",
                    date.ToString("dd/MM/yyyy"), attempt, this.retryCount);

                using var scope = this.scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ICurrencyRateService>();
                var allSynced = await service.SyncRatesAsync(date);

                if (allSynced)
                {
                    this.logger.LogInformation(
                        "Синхронізація {Date}: всі курси успішно отримані (спроба {Attempt})",
                        date.ToString("dd/MM/yyyy"), attempt);
                    return;
                }

                if (attempt < this.retryCount)
                {
                    this.logger.LogWarning(
                        "Синхронізація {Date}: не всі курси отримані. Наступна спроба через {Hours}г",
                        date.ToString("dd/MM/yyyy"), retryInterval.TotalHours);

                    await Task.Delay(retryInterval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex,
                    "Помилка синхронізації {Date} (спроба {Attempt} з {Total})",
                    date.ToString("dd/MM/yyyy"), attempt, this.retryCount);

                if (attempt < this.retryCount)
                {
                    await Task.Delay(retryInterval, stoppingToken);
                }
            }
        }

        this.logger.LogWarning(
            "Синхронізація {Date}: вичерпано всі {Total} спроби",
            date.ToString("dd/MM/yyyy"), this.retryCount);
    }

    private TimeSpan CalculateDelayUntilNextSync()
    {
        var now = DateTime.Now;
        var todaySync = DateTime.Today.Add(this.syncTime.ToTimeSpan());
        var nextSync = now < todaySync ? todaySync : todaySync.AddDays(1);
        return nextSync - now;
    }
}
