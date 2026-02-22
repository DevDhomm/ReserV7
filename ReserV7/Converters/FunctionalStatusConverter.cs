using System.Globalization;
using System.Windows.Data;

namespace ReserV7.Converters
{
    public class FunctionalStatusConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isFunctional = (value as bool?) == true;
            return isFunctional ? "✓ Fonctionnel" : "✗ En panne/Maintenance";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
