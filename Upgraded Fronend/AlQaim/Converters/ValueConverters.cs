using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AlQaim.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status)
                {
                    case "Approved":
                        return new SolidColorBrush(Color.FromRgb(39, 174, 96)); // Green
                    case "Pending":
                        return new SolidColorBrush(Color.FromRgb(243, 156, 18)); // Orange
                    case "Rejected":
                        return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Red
                    default:
                        return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // Gray
                }
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StatusToCancelVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string status = value as string;
            // Only allow cancelling if status is "Pending"
            return status == "Pending" ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringIsNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class AlternatingRowBackgroundConverter : IValueConverter
    {
        public Brush EvenRowBrush { get; set; } = Brushes.White; // Default
        public Brush OddRowBrush { get; set; } = Brushes.LightGray; // Default

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int alternationIndex)
            {
                return alternationIndex % 2 == 0 ? EvenRowBrush : OddRowBrush;
            }
            return EvenRowBrush; // Default if conversion fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
