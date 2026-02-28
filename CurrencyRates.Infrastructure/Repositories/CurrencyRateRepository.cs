using CurrencyRates.Domain.Entities;
using CurrencyRates.Domain.Enums;
using CurrencyRates.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using SqlKata.Execution;

namespace CurrencyRates.Infrastructure.Repositories;

/// <summary>
/// Реалізація репозиторію через SqlKata.
/// </summary>
public class CurrencyRateRepository : ICurrencyRateRepository
{
    private readonly QueryFactory _db;
    private readonly ILogger<CurrencyRateRepository> _logger;

    public CurrencyRateRepository(QueryFactory db, ILogger<CurrencyRateRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CurrencyRate>> GetByDateAsync(DateOnly date)
    {
        var rows = await _db.Query("CurrencyRates as cr")
            .Join("Currencies as c", "c.Id", "cr.CurrencyId")
            .Select(
                "cr.Id",
                "cr.CurrencyId",
                "cr.Rate",
                "cr.RateDate",
                "cr.Source",
                "cr.CreatedAt",
                "c.Id as Currency_Id",
                "c.Code as Currency_Code",
                "c.Name as Currency_Name")
            .Where("cr.RateDate", date.ToString("yyyy-MM-dd"))
            .GetAsync();

        return rows.Select(row => new CurrencyRate
        {
            Id = (int)row.Id,
            CurrencyId = (int)row.CurrencyId,
            Rate = (decimal)row.Rate,
            RateDate = DateOnly.FromDateTime((DateTime)row.RateDate),
            Source = Enum.Parse<SourceType>(row.Source.ToString()),
            CreatedAt = (DateTime)row.CreatedAt,
            Currency = new Currency
            {
                Id = (int)row.Currency_Id,
                Code = row.Currency_Code.ToString(),
                Name = row.Currency_Name.ToString()
            }
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<int?> GetCurrencyIdByCodeAsync(string code)
    {
        var result = await _db.Query("Currencies")
            .Select("Id")
            .Where("Code", code)
            .FirstOrDefaultAsync<int?>();

        return result;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetExistingCurrencyCodesAsync(DateOnly date)
    {
        var codes = await _db.Query("CurrencyRates as cr")
            .Join("Currencies as c", "c.Id", "cr.CurrencyId")
            .Select("c.Code")
            .Where("cr.RateDate", date.ToString("yyyy-MM-dd"))
            .GetAsync<string>();

        return codes.ToList();
    }

    /// <inheritdoc/>
    public async Task SaveRatesAsync(IEnumerable<CurrencyRate> rates, SourceType source)
    {
        foreach (var rate in rates)
        {
            try
            {
                var exists = await _db.Query("CurrencyRates")
                    .Where("CurrencyId", rate.CurrencyId)
                    .Where("RateDate", rate.RateDate.ToString("yyyy-MM-dd"))
                    .ExistsAsync();

                if (exists)
                {
                    _logger.LogInformation(
                        "Курс для {Code} на {Date} вже є в БД, пропускаємо",
                        rate.Currency?.Code ?? $"CurrencyId={rate.CurrencyId}", rate.RateDate);
                    continue;
                }

                await _db.Query("CurrencyRates").InsertAsync(new
                {
                    CurrencyId = rate.CurrencyId,
                    Rate = rate.Rate,
                    RateDate = rate.RateDate.ToString("yyyy-MM-dd"),
                    Source = source.ToString(),
                    CreatedAt = DateTime.Now
                });

                _logger.LogInformation(
                    "Збережено курс {Code} = {Rate} на {Date}",
                    rate.Currency?.Code ?? $"CurrencyId={rate.CurrencyId}", rate.Rate, rate.RateDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Помилка збереження курсу {Code} на {Date}",
                    rate.CurrencyId, rate.RateDate);
            }
        }
    }
}
