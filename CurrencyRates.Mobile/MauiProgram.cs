using CurrencyRates.Mobile.Services;
using CurrencyRates.Mobile.ViewModels;
using CurrencyRates.Mobile.Views;
using Microsoft.Extensions.Logging;
using Plugin.Firebase.CloudMessaging;

namespace CurrencyRates.Mobile;

/// <summary>
/// Entry point for configuring and building the MAUI application.
/// Registers services, view models, and views in the DI container.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Default base URL for the Currency Rates API.
    /// </summary>
    private const string ApiBaseUrl = "http://34.253.10.242:8080";

    /// <summary>
    /// Creates and configures the MAUI application.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/> instance.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register HttpClient with base address for the API client
        builder.Services.AddHttpClient<ICurrencyRateApiClient, CurrencyRateApiClient>(client =>
        {
            client.BaseAddress = new Uri(ApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // Register sync notification service (FCM + SignalR)
        builder.Services.AddSingleton<ISyncNotificationService>(sp =>
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
            return new SyncNotificationService(httpClient, ApiBaseUrl);
        });

        // Register ViewModels
        builder.Services.AddTransient<CurrencyRatesViewModel>();

        // Register Views and Shell
        builder.Services.AddTransient<AppShell>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
