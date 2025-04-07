using System;
using System.Globalization;
using System.Windows; 
using System.Windows.Data;

namespace AlQaim.Converters 
{
    public class ClearWatermarkOnFocusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is string currentText && values[1] is string watermark)
            {
               
                return currentText == watermark ? "" : currentText;
            }
         
            return values.Length > 0 ? values[0] : Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // Not needed for one-way conversion like this
            throw new NotImplementedException();
        }
    }
}