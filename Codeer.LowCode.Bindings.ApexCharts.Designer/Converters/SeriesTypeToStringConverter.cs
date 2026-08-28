using System.Globalization;
using System.Windows.Data;
using Codeer.LowCode.Bindings.ApexCharts.Models;

namespace Codeer.LowCode.Bindings.ApexCharts.Designer.Converters
{
    //SeriesType とローカライズ済み表示名の相互変換 (コンボの候補は ChartSeriesViewModel.TypeCandidates=表示名)
    public class SeriesTypeToStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is SeriesType type ? type.GetDisplayName() : "";

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => SeriesTypeExtensions.ParseDisplayName(value as string);
    }
}
