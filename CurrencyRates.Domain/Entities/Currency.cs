using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyRates.Domain.Entities;

/// <summary>
/// Довідник валют. Містить унікальний код валюти та її назву.
/// Наприклад "USD" "Американський долар".
/// </summary>
public class Currency
{
    public int Id { get; set; }

    /// <summary>
    /// Код валюти, наприклад "USD", "EUR", "UAH". Унікальний ідентифікатор валюти.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Назва валюти, наприклад "Американський долар", "Євро", "Українська гривня".
    /// </summary>
    public string Name { get; set; }
}
