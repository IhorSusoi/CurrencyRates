namespace CurrencyRates.Domain.Entities;

/// <summary>
/// Stores an FCM device token for push notification delivery.
/// Each mobile device registers its token on startup so the server
/// can send push notifications when new rates are synced.
/// </summary>
public class DeviceToken
{
    public int Id { get; set; }

    /// <summary>
    /// The FCM registration token unique to the device.
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// Platform identifier, e.g. "Android".
    /// </summary>
    public string Platform { get; set; } = null!;

    /// <summary>
    /// When this token was first registered.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
