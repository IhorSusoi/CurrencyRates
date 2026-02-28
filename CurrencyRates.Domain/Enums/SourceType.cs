using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyRates.Domain.Enums;

/// <summary>
/// Визначає вручну введені дані чи автоматично.
/// </summary>
public enum SourceType
{
    /// <summary>
    /// Завантажено автоматично по графіку.
    /// </summary>
    Auto,
    /// <summary>
    /// Завантажено на вимогу користувача.
    /// </summary>
    Manual
}
