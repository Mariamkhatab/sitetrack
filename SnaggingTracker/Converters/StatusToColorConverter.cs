using SnaggingTracker.Helpers;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SnaggingTracker.Converters
{
    /// <summary>
    /// Converts an IssueStatus enum to a SolidColorBrush.
    /// Open = Red, InProgress = Blue, Closed = Green.
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IssueStatus status)
            {
                return status switch
                {
                    IssueStatus.Open       => new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
                    IssueStatus.InProgress => new SolidColorBrush(Color.FromRgb(0x21, 0x74, 0xAE)),
                    IssueStatus.Closed     => new SolidColorBrush(Color.FromRgb(0x1E, 0x84, 0x49)),
                    _                     => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
