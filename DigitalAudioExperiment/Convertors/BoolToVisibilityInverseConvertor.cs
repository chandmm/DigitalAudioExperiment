using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DigitalAudioExperiment.Convertors
{
    public class BoolToVisibilityInverseConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool actualValue)
            {
                return actualValue 
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
