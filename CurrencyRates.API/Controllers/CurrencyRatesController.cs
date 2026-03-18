using CurrencyRates.API.DTOs;
using CurrencyRates.Application.DTOs;
using CurrencyRates.Domain.Enums;
using CurrencyRates.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyRates.API.Controllers;

/// <summary>
/// Контролер для роботи з курсами валют.
/// </summary>
[ApiController]
[Route("api/currency-rates")]
public class CurrencyRatesController : ControllerBase
{
    private readonly ICurrencyRateService _currencyRateService;
    private readonly ILogger<CurrencyRatesController> _logger;

    public CurrencyRatesController(
        ICurrencyRateService currencyRateService,
        ILogger<CurrencyRatesController> logger)
    {
        _currencyRateService = currencyRateService;
        _logger = logger;
    }

    /// <summary>
    /// Повертає курси валют на вказану дату.
    /// Якщо даних в БД немає або вони неповні — автоматично завантажує з НБУ.
    /// </summary>
    /// <param name="date">Дата у форматі yyyy-MM-dd.</param>
    /// <returns>Список курсів валют.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CurrencyRateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetRates([FromQuery] DateOnly date)
    {
        _logger.LogInformation("Запит курсів на дату {Date}", date.ToString("dd/MM/yyyy"));

        try
        {
            var rates = await _currencyRateService.GetOrFetchRatesAsync(date, SourceType.Manual);

            var result = rates.Select(r => new CurrencyRateDto
            {
                CurrencyCode = r.Currency.Code,
                CurrencyName = r.Currency.Name,
                Rate = r.Rate,
                RateDate = r.RateDate,
                Source = r.Source.ToString(),
                CreatedAt = r.CreatedAt
            });

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            // НБУ недоступний
            _logger.LogError(ex, "НБУ API недоступний при запиті на дату {Date}", date.ToString("dd/MM/yyyy"));

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Не вдалося отримати дані з НБУ. Спробуйте пізніше.");
        }
    }

    /// <summary>
    /// Triggers a manual sync of currency rates for today.
    /// </summary>
    /// <returns>Result indicating whether all rates were synced.</returns>
    [HttpPost("sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SyncRates()
    {
        _logger.LogInformation("Manual sync requested");

        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var result = await _currencyRateService.SyncRatesAsync(today);
            return Ok(new { synced = result });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "NBU API unavailable during manual sync");
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Failed to sync rates from NBU. Try again later.");
        }
    }
}

/// <summary>
/// Controller for managing device tokens used for FCM push notifications.
/// </summary>
[ApiController]
[Route("api/device-tokens")]
public class DeviceTokensController : ControllerBase
{
    private readonly IDeviceTokenRepository _deviceTokenRepository;
    private readonly ILogger<DeviceTokensController> _logger;

    public DeviceTokensController(
        IDeviceTokenRepository deviceTokenRepository,
        ILogger<DeviceTokensController> logger)
    {
        _deviceTokenRepository = deviceTokenRepository;
        _logger = logger;
    }

    /// <summary>
    /// Registers a device FCM token for push notifications.
    /// </summary>
    /// <param name="request">The token and platform info.</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterToken([FromBody] API.DTOs.RegisterDeviceTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest("Token is required.");

        await _deviceTokenRepository.SaveTokenAsync(request.Token, request.Platform);
        _logger.LogInformation("Device token registered for {Platform}", request.Platform);

        return Ok();
    }

    /// <summary>
    /// Removes a device FCM token.
    /// </summary>
    /// <param name="token">The token to remove.</param>
    [HttpDelete("{token}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveToken(string token)
    {
        await _deviceTokenRepository.DeleteTokenAsync(token);
        return Ok();
    }
}
