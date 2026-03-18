using CurrencyRates.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using SqlKata.Execution;

namespace CurrencyRates.Infrastructure.Repositories;

/// <summary>
/// SqlKata implementation for managing FCM device tokens.
/// </summary>
public class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly QueryFactory db;
    private readonly ILogger<DeviceTokenRepository> logger;

    public DeviceTokenRepository(QueryFactory db, ILogger<DeviceTokenRepository> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetAllTokensAsync()
    {
        var tokens = await this.db.Query("DeviceTokens")
            .Select("Token")
            .GetAsync<string>();

        return tokens.ToList();
    }

    /// <inheritdoc/>
    public async Task SaveTokenAsync(string token, string platform)
    {
        var exists = await this.db.Query("DeviceTokens")
            .Where("Token", token)
            .ExistsAsync();

        if (!exists)
        {
            await this.db.Query("DeviceTokens").InsertAsync(new
            {
                Token = token,
                Platform = platform,
                CreatedAt = DateTime.Now
            });

            this.logger.LogInformation("Registered new device token for platform {Platform}", platform);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteTokenAsync(string token)
    {
        var deleted = await this.db.Query("DeviceTokens")
            .Where("Token", token)
            .DeleteAsync();

        if (deleted > 0)
        {
            this.logger.LogInformation("Removed device token");
        }
    }
}
