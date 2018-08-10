using System;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace GBCLV2.Helpers
{
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var time = (TimeSpan)value;

            if(time != TimeSpan.Zero)
            {
                return $"{time.TotalSeconds}s";
            }
            else return "0.1s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;

            if (str.EndsWith("s"))
            {
                str = str.TrimEnd('s');
            }

            if (double.TryParse(str, out double seconds))
            {
                if (seconds < 0.05)
                {
                    seconds = 0.05;
                }

                return new TimeSpan((long)(seconds * 10000000));
            }

            return new TimeSpan(5000000);
        }
    }

    public class NegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CodePageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch((int)value)
            {
                case 20127: return "ASCII";
                case 936: return "GBK";
                case 65001: return "UTF-8";
                case 1200: return "Unicode";
                default: return "GBK";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch(value as string)
            {
                case "ASCII": return 20127;
                case "GBK": return 936;
                case "UTF-8": return 65001;
                case "Unicode": return 1200;
                default: return 936;
            }
        }
    }

    public class LineBreakConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value as string)
            {
                case "\n": return "\\n (LF)";
                case "\r": return "\\r (CR)";
                case "\r\n": return "\\r\\n (CRLF)";
                default: return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value as string)
            {
                case "\\n (LF)": return "\n";
                case "\\r (CR)": return "\r";
                case "\\r\\n (CRLF)": return "\r\n";
                default: return null;
            }
        }
    }

    public class ParityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.GetName(typeof(Parity), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(Parity), (string)value);
        }
    }

    public class StopBitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.GetName(typeof(StopBits), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(typeof(StopBits), (string)value);
        }
    }
}
