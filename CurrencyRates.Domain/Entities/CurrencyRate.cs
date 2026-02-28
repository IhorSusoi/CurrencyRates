using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyRates.Domain.Enums;

namespace CurrencyRates.Domain.Entities;

/// <summary>
/// Курс однієї валюти відносно гривні на конкретну дату. Містить інформацію про валюту, дату, курс та джерело даних.
/// </summary>
public class CurrencyRate
{
    public int Id { get; set; }
    /// <summary>
    /// Ідентифікатор валюти, до якої відноситься цей курс.
    /// </summary>
    public int CurrencyId { get; set; }
    /// <summary>
    /// Навігаційна властивість для зв'язку з таблицею валют
    /// </summary>
    public Currency Currency { get; set; } = null!;
    /// <summary>
    /// Курс валюти відносно гривні на вказану дату. Наприклад, 27.5 означає, що 1 одиниця валюти коштує 27.5 гривень.
    /// </summary>
    public decimal Rate { get; set; }
    /// <summary>
    /// Дата, на яку встановлено курс. Формат "yyyy-MM-dd".
    /// </summary>
    public DateTime RateDate { get; set; }
    /// <summary>
    /// Джерело даних, яке вказує, чи був курс завантажений автоматично по графіку (Auto) чи введений вручну користувачем (Manual).
    /// </summary>
    public Enums.SourceType Source { get; set; }
    /// <summary>
    /// Коли запис був збережений у базі даних.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
