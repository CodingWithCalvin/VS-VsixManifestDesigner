using System;
using System.Globalization;
using System.Windows.Data;

namespace CodingWithCalvin.VsixManifestDesigner.Converters;

/// <summary>
/// Converts an index value to a boolean, returning true if the bound value equals the converter parameter.
/// Used for binding RadioButtons to a SelectedTabIndex property.
/// </summary>
public sealed class IndexToBoolConverter : IValueConverter
{
    /// <summary>
    /// Converts an integer index to a boolean by comparing with the parameter.
    /// </summary>
    /// <param name="value">The current index value.</param>
    /// <param name="targetType">The target type (bool).</param>
    /// <param name="parameter">The index to compare against.</param>
    /// <param name="culture">The culture info.</param>
    /// <returns>True if value equals parameter; otherwise, false.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int index && parameter is string paramString && int.TryParse(paramString, out int paramIndex))
        {
            return index == paramIndex;
        }

        return false;
    }

    /// <summary>
    /// Converts a boolean back to an integer index.
    /// </summary>
    /// <param name="value">The boolean value (IsChecked).</param>
    /// <param name="targetType">The target type (int).</param>
    /// <param name="parameter">The index value to return if true.</param>
    /// <param name="culture">The culture info.</param>
    /// <returns>The parameter value if true; otherwise, Binding.DoNothing.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is string paramString && int.TryParse(paramString, out int paramIndex))
        {
            return paramIndex;
        }

        return Binding.DoNothing;
    }
}
