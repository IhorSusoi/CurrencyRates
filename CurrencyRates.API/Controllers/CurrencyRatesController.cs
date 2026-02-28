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
        _logger.LogInformation("Запит курсів на дату {Date}", date);

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
            _logger.LogError(ex, "НБУ API недоступний при запиті на дату {Date}", date);

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Не вдалося отримати дані з НБУ. Спробуйте пізніше.");
        }
    }
}
