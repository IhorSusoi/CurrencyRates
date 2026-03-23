namespace CurrencyRates.Domain.Interfaces;

/// <summary>
/// Repository for managing FCM device tokens used for push notifications.
/// </summary>
public interface IDeviceTokenRepository
{
    /// <summary>
    /// Returns all registered device tokens.
    /// </summary>
    Task<IReadOnlyList<string>> GetAllTokensAsync();

    /// <summary>
    /// Saves a device token. If the token already exists, does nothing.
    /// </summary>
    /// <param name="token">The FCM token.</param>
    /// <param name="platform">Platform name, e.g. "Android".</param>
    Task SaveTokenAsync(string token, string platform);

    /// <summary>
    /// Removes a device token (e.g. when it becomes invalid).
    /// </summary>
    /// <param name="token">The FCM token to remove.</param>
    Task DeleteTokenAsync(string token);
}
