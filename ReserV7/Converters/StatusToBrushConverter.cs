using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ReserV7.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "En attente" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC00")), // Jaune
                    "Confirmée" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D4")), // Bleu
                    "En cours" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#107C10")), // Vert
                    "Terminée" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")), // Gris
                    "Annulée" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D83B01")), // Rouge
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D4")) // Bleu par défaut
                };
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D4"));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToForegroundBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "En attente" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")), // Noir pour meilleure lisibilité sur jaune
                    "Confirmée" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")), // Blanc
                    "En cours" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")), // Blanc
                    "Terminée" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")), // Blanc
                    "Annulée" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")), // Blanc
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")) // Blanc par défaut
                };
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToEnabledConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                // Desactive le boutton d'edit pour "Terminée", "En cours", et "Annulée"
                return status != "Terminée" && status != "En cours" && status != "Annulée";
            }

            return true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
