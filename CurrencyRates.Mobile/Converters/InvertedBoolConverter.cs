using System.Globalization;

namespace CurrencyRates.Mobile.Converters;

/// <summary>
/// Converts a boolean value to its inverse. Used in XAML bindings.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return false;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return false;
    }
}
