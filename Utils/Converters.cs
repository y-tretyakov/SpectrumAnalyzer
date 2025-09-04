using System.Globalization;
using System.Windows.Data;

namespace SpectrumAnalyzer
{
    /// <summary>
    /// Converts spectrum amplitude values (0-1) to pixel heights for main spectrum bars
    /// </summary>
    public class HeightConverter : IValueConverter
    {
        public static readonly HeightConverter Instance = new();

        private const double MaxHeight = 200.0; // Maximum bar height in pixels
        private const double MinHeight = 2.0;   // Minimum visible height

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double amplitude)
            {
                // Convert amplitude (0-1) to pixel height
                double height = Math.Max(MinHeight, amplitude * MaxHeight);
                return height;
            }
            
            return MinHeight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts spectrum amplitude values (0-1) to pixel heights for reflection bars
    /// </summary>
    public class ReflectionHeightConverter : IValueConverter
    {
        public static readonly ReflectionHeightConverter Instance = new();

        private const double MaxHeight = 100.0; // Maximum reflection height (50% of main)
        private const double MinHeight = 1.0;   // Minimum visible height

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double amplitude)
            {
                // Convert amplitude (0-1) to pixel height for reflection
                double height = Math.Max(MinHeight, amplitude * MaxHeight);
                return height;
            }
            
            return MinHeight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}