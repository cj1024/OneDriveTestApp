using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace OneDriveExtentions.Converters
{
    public class ByteCountToStringConverter : IValueConverter
    {

        private static readonly IList<string> UnitList = new[] { "Byte", "KB", "MB", "GB", "TB", "PB" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double longValue;
            if (double.TryParse(value.ToString(), out longValue))
            {
                var unit = UnitList.GetEnumerator();
                while (unit.MoveNext())
                {
                    if (longValue < 1024)
                    {
                        return string.Format("{0:0.##} {1}", longValue, unit.Current);
                    }
                    longValue = longValue / 1024;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }
}
