using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Plugin.Firebase.CloudMessaging;

namespace CurrencyRates.Mobile.Services;

/// <summary>
/// Manages FCM token registration with the backend and
/// maintains a SignalR connection for in-app real-time updates.
/// FCM handles push notifications when the app is closed;
/// SignalR handles real-time updates when the app is open.
/// </summary>
public class SyncNotificationService : ISyncNotificationService
{
    private readonly HttpClient httpClient;
    private readonly string hubUrl;
    private HubConnection? hubConnection;

    /// <inheritdoc/>
    public event Action<string, int>? OnRatesSynced;

    /// <summary>
    /// Initializes a new instance of <see cref="SyncNotificationService"/>.
    /// </summary>
    /// <param name="httpClient">HTTP client for registering FCM tokens with the API.</param>
    /// <param name="apiBaseUrl">Base URL of the API server.</param>
    public SyncNotificationService(HttpClient httpClient, string apiBaseUrl)
    {
        this.httpClient = httpClient;
        this.hubUrl = $"{apiBaseUrl.TrimEnd('/')}/hubs/currency-rates";
    }

    /// <inheritdoc/>
    public async Task StartAsync()
    {
        // 1. Register FCM token with the backend
        await this.RegisterFcmTokenAsync();

        // 2. Start SignalR connection for in-app real-time updates
        await this.StartSignalRAsync();
    }

    /// <inheritdoc/>
    public async Task StopAsync()
    {
        if (this.hubConnection is not null)
        {
            await this.hubConnection.StopAsync();
            await this.hubConnection.DisposeAsync();
            this.hubConnection = null;
        }
    }

    /// <summary>
    /// Gets the FCM token from Firebase and registers it with the backend API.
    /// </summary>
    private async Task RegisterFcmTokenAsync()
    {
        try
        {
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                var request = new { Token = token, Platform = "Android" };
                await this.httpClient.PostAsJsonAsync("api/device-tokens", request);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FCM token registration failed: {ex}");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var page = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (page is not null)
                    await page.DisplayAlertAsync("FCM Debug", $"Token registration failed: {ex.Message}", "OK");
            });
        }
    }

    /// <summary>
    /// Establishes a SignalR connection and subscribes to the RatesSynced event.
    /// </summary>
    private async Task StartSignalRAsync()
    {
        try
        {
            if (this.hubConnection is not null)
                return;

            this.hubConnection = new HubConnectionBuilder()
                .WithUrl(this.hubUrl)
                .WithAutomaticReconnect()
                .Build();

            this.hubConnection.On<dynamic>("RatesSynced", (payload) =>
            {
                try
                {
                    string date = payload.GetProperty("date").GetString() ?? "";
                    int ratesCount = payload.GetProperty("ratesCount").GetInt32();
                    this.OnRatesSynced?.Invoke(date, ratesCount);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error parsing SignalR message: {ex.Message}");
                }
            });

            await this.hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SignalR connection failed: {ex.Message}");
        }
    }
}
