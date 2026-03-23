namespace CurrencyRates.Domain.Interfaces;

/// <summary>
/// Abstracts notification delivery when currency rates are synced.
/// The Application layer calls this interface without knowing whether
/// notifications go via SignalR, FCM, or any other channel.
/// </summary>
public interface IRateSyncNotifier
{
    /// <summary>
    /// Notifies all subscribed clients that new rates have been synced.
    /// </summary>
    /// <param name="date">The date for which rates were synced.</param>
    /// <param name="ratesCount">Number of rates that were synced.</param>
    Task NotifySyncCompletedAsync(DateOnly date, int ratesCount);
}
