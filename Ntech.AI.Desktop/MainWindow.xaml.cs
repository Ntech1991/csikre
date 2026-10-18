using System.Windows;
using System.Windows.Data;
using Ntech.AI.Common.Models;

namespace Ntech.AI.Desktop
{
    /// <summary>
    /// Converter to convert MessageRole to Color
    /// </summary>
    public class RoleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MessageRole role)
            {
                return role switch
                {
                    MessageRole.User => System.Windows.Media.Brushes.Blue,
                    MessageRole.Assistant => System.Windows.Media.Brushes.Green,
                    MessageRole.System => System.Windows.Media.Brushes.Red,
                    _ => System.Windows.Media.Brushes.Black
                };
            }
            return System.Windows.Media.Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter to invert boolean
    /// </summary>
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}