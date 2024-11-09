using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DigitalAudioExperiment.Convertors
{
    public class VisibilityToGridLengthConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterValue = parameter as string;
            var size = 0;

            if (string.IsNullOrEmpty(parameterValue))
            {
                size = 0;
            }
            else if (!Int32.TryParse((string)parameter, out size))
            {
                size = 0;
            }

            var visibility = value as Visibility?;

            if (visibility == null)
            {
                return new GridLength(size);
            }

            if (visibility == Visibility.Visible)
            {
                return new GridLength(size);
            }

            return new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
