using Microsoft.AspNetCore.SignalR;

namespace CurrencyRates.API.Hubs;

/// <summary>
/// SignalR hub for real-time currency rate notifications.
/// The server broadcasts "RatesSynced" messages to all connected clients
/// (Blazor web app) when new rates are synced.
/// The hub itself has no client-callable methods — all communication
/// is server-to-client via IHubContext.
/// </summary>
public class CurrencyRateHub : Hub
{
}
