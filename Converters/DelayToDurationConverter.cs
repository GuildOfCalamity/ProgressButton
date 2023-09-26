using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressButtonDemo;

public class DelayToDurationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        double factor = 0.5;

        if (!Enum.IsDefined(typeof(DelayTime), value))
            throw new ArgumentException("DelayToDurationConverter: value must be an enum");

        if (parameter != null && parameter is string optional)
        {
            // You should adjust the factor based on your enum values.
            if (double.TryParse(optional, out var amount))
            {
                System.Diagnostics.Debug.WriteLine($"Detected parameter: {amount}");
                factor = amount;
            }
        }

        try
        {
            var enumValue = (DelayTime)Enum.Parse(typeof(DelayTime), $"{value}");
            return new Duration(TimeSpan.FromSeconds(factor * (int)enumValue));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DelayToDurationConverter: {ex.Message}");
            return new Duration(TimeSpan.FromSeconds(1));
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}
