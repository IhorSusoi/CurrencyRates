using CurrencyRates.API.Hubs;
using CurrencyRates.Domain.Interfaces;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace CurrencyRates.API.Services;

/// <summary>
/// Sends sync notifications via SignalR (to Blazor clients)
/// and Firebase Cloud Messaging (to mobile devices).
/// </summary>
public class RateSyncNotifier : IRateSyncNotifier
{
    private readonly IHubContext<CurrencyRateHub> hubContext;
    private readonly IDeviceTokenRepository deviceTokenRepository;
    private readonly ILogger<RateSyncNotifier> logger;

    public RateSyncNotifier(
        IHubContext<CurrencyRateHub> hubContext,
        IDeviceTokenRepository deviceTokenRepository,
        ILogger<RateSyncNotifier> logger)
    {
        this.hubContext = hubContext;
        this.deviceTokenRepository = deviceTokenRepository;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task NotifySyncCompletedAsync(DateOnly date, int ratesCount)
    {
        // 1. Notify Blazor clients via SignalR
        await this.SendSignalRNotificationAsync(date, ratesCount);

        // 2. Notify mobile devices via FCM
        await this.SendFcmNotificationAsync(date, ratesCount);
    }

    /// <summary>
    /// Broadcasts a "RatesSynced" message to all connected SignalR clients.
    /// </summary>
    private async Task SendSignalRNotificationAsync(DateOnly date, int ratesCount)
    {
        try
        {
            await this.hubContext.Clients.All.SendAsync("RatesSynced", new
            {
                date = date.ToString("yyyy-MM-dd"),
                ratesCount
            });

            this.logger.LogInformation(
                "SignalR notification sent: {RatesCount} rates synced for {Date}",
                ratesCount, date.ToString("dd/MM/yyyy"));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to send SignalR notification");
        }
    }

    /// <summary>
    /// Sends push notifications to all registered mobile devices via FCM.
    /// Automatically removes invalid tokens.
    /// </summary>
    private async Task SendFcmNotificationAsync(DateOnly date, int ratesCount)
    {
        try
        {
            var tokens = await this.deviceTokenRepository.GetAllTokensAsync();

            if (tokens.Count == 0)
            {
                this.logger.LogInformation("No device tokens registered, skipping FCM");
                return;
            }

            var message = new MulticastMessage
            {
                Tokens = tokens.ToList(),
                Notification = new Notification
                {
                    Title = "Currency Rates Updated",
                    Body = $"{ratesCount} rates synced for {date:dd/MM/yyyy}"
                },
                Data = new Dictionary<string, string>
                {
                    ["date"] = date.ToString("yyyy-MM-dd"),
                    ["ratesCount"] = ratesCount.ToString()
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

            this.logger.LogInformation(
                "FCM notification sent: {Success} success, {Failure} failed out of {Total}",
                response.SuccessCount, response.FailureCount, tokens.Count);

            // Remove invalid tokens
            for (int i = 0; i < response.Responses.Count; i++)
            {
                if (!response.Responses[i].IsSuccess)
                {
                    var errorCode = response.Responses[i].Exception?.MessagingErrorCode;
                    if (errorCode == MessagingErrorCode.Unregistered ||
                        errorCode == MessagingErrorCode.InvalidArgument)
                    {
                        await this.deviceTokenRepository.DeleteTokenAsync(tokens[i]);
                        this.logger.LogWarning("Removed invalid FCM token at index {Index}", i);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to send FCM notifications");
        }
    }
}
