namespace CurrencyRates.API.DTOs;

/// <summary>
/// Request body for registering a device FCM token.
/// </summary>
public class RegisterDeviceTokenRequest
{
    /// <summary>
    /// The FCM registration token from the mobile device.
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// Platform name, e.g. "Android".
    /// </summary>
    public string Platform { get; set; } = "Android";
}
