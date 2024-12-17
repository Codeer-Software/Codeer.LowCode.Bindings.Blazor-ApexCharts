using System.Globalization;
using System.Windows.Data;
using ApexCharts;

namespace Codeer.LowCode.Bindings.ApexCharts.Designer.Converters
{
    public class SeriesTypeToStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value?.ToString() ?? "";

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Enum.TryParse<SeriesType>((string?)value, true, out var type) ? type : null;
    }
}
