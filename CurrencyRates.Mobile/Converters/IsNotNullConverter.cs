using System.Globalization;

namespace CurrencyRates.Mobile.Converters;

/// <summary>
/// Returns true if the value is not null. Used to control visibility in XAML.
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
