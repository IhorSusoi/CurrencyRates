using CurrencyRates.Application.Options;
using CurrencyRates.Application.Services;
using CurrencyRates.Domain.Interfaces;
using CurrencyRates.Infrastructure.Data;
using CurrencyRates.Infrastructure.ExternalServices;
using CurrencyRates.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace CurrencyRates.Infrastructure.Extensions;

/// <summary>
/// Extension методи для реєстрації Infrastructure сервісів в DI контейнері.
/// Викликається один раз в Program.cs API проєкту.
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        // --- EF Core (тільки для міграцій) ---
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        // --- SqlKata ---
        services.AddScoped<QueryFactory>(_ =>
        {
            var connection = new SqlConnection(connectionString);
            return new QueryFactory(connection, new SqlServerCompiler());
        });

        // --- Репозиторій ---
        services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();

        // --- НБУ API клієнт з Polly retry ---
        var currencyOptions = configuration.GetSection(CurrencyOptions.SectionName)
            .Get<CurrencyOptions>();

        services.AddHttpClient<INbuApiClient, NbuApiClient>(client =>
        {
            client.BaseAddress = new Uri(currencyOptions.NbuApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddTransientHttpErrorPolicy(policy =>
            // 3 спроби з затримкою 30 → 60 → 120 секунд
            policy.WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(30 * Math.Pow(2, attempt - 1)),
                onRetry: (outcome, timespan, attempt, _) =>
                {
                    Console.WriteLine(
                        $"[Polly] Спроба {attempt} для НБУ після {timespan.TotalSeconds}с. " +
                        $"Причина: {outcome.Exception?.Message}");
                }));

        // --- Сервіси Application шару ---
        services.AddScoped<ICurrencyRateService, CurrencyRateService>();

        // --- Фонова служба автоматичної синхронізації ---
        services.AddHostedService<AutoSyncHostedService>();

        // --- Налаштування валют з appsettings.json ---
        services.Configure<CurrencyOptions>(
            configuration.GetSection(CurrencyOptions.SectionName));

        return services;
    }
}
