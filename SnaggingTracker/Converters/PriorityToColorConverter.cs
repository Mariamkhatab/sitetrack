using SnaggingTracker.Helpers;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SnaggingTracker.Converters
{
    /// <summary>
    /// Converts an IssuePriority enum value to a SolidColorBrush for UI coloring.
    /// High = Red, Medium = Amber, Low = Green.
    /// </summary>
    public class PriorityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IssuePriority priority)
            {
                return priority switch
                {
                    IssuePriority.High   => new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
                    IssuePriority.Medium => new SolidColorBrush(Color.FromRgb(0xE6, 0x7E, 0x22)),
                    IssuePriority.Low    => new SolidColorBrush(Color.FromRgb(0x1E, 0x84, 0x49)),
                    _                   => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
