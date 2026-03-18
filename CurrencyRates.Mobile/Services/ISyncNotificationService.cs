namespace CurrencyRates.Mobile.Services;

/// <summary>
/// Manages FCM registration and SignalR connection for receiving
/// real-time sync notifications from the server.
/// </summary>
public interface ISyncNotificationService
{
    /// <summary>
    /// Initializes FCM token registration and starts the SignalR connection.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stops the SignalR connection.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Fired when the server broadcasts that new rates have been synced.
    /// Parameters: date string (yyyy-MM-dd), number of rates synced.
    /// </summary>
    event Action<string, int>? OnRatesSynced;
}
