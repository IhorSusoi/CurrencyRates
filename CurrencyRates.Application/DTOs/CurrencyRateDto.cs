using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyRates.Application.DTOs;

/// <summary>
/// Об'єкт передачі даних (DTO) для курсу валюти.
/// </summary>
public class CurrencyRateDto
{
    /// <summary>Код валюти (USD, EUR, DKK, PLN).</summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>Назва валюти (Американський долар, Євро, Датська крона, Польський злотий).</summary>
    public string CurrencyName { get; set; } = string.Empty;

    /// <summary>Курс відносно гривні.</summary>
    public decimal Rate { get; set; }

    /// <summary>Дата на яку діє курс.</summary>
    public DateOnly RateDate { get; set; }

    /// <summary>Звідки прийшов курс: Auto або Manual.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Коли запис був збережений в БД.</summary>
    public DateTime CreatedAt { get; set; }
}