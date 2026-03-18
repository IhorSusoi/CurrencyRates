using Microsoft.AspNetCore.SignalR.Client;

namespace CurrencyRates.Client.Services;

/// <summary>
/// Manages the SignalR connection for receiving real-time sync notifications.
/// Blazor components subscribe to the OnRatesSynced event to react
/// when the server pushes new rate updates.
/// </summary>
public class SyncNotificationService : IAsyncDisposable
{
    private HubConnection? hubConnection;

    /// <summary>
    /// Fired when the server broadcasts that new rates have been synced.
    /// Parameters: date string (yyyy-MM-dd), number of rates synced.
    /// </summary>
    public event Action<string, int>? OnRatesSynced;

    /// <summary>
    /// Starts the SignalR connection to the currency rate hub.
    /// </summary>
    /// <param name="hubUrl">Full URL of the SignalR hub, e.g. "http://localhost/hubs/currency-rates".</param>
    public async Task StartAsync(string hubUrl)
    {
        if (this.hubConnection is not null)
            return;

        this.hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        this.hubConnection.On<dynamic>("RatesSynced", (payload) =>
        {
            string date = payload.GetProperty("date").GetString() ?? "";
            int ratesCount = payload.GetProperty("ratesCount").GetInt32();
            this.OnRatesSynced?.Invoke(date, ratesCount);
        });

        await this.hubConnection.StartAsync();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (this.hubConnection is not null)
        {
            await this.hubConnection.DisposeAsync();
        }
    }
}
