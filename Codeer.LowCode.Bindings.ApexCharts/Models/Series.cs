using ApexCharts;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Models
{
    public class ChartSeries : ICurrentSettingsText
    {
        public List<Series> Series { get; set; } = [];

        public string GetCurrentSettings() => string.Join(", ", Series.Select(s => s.Name));
    }

    public class Series
    {
        public string Name { get; set; } = string.Empty;
        public SeriesType Type { get; set; } = SeriesType.Line;
    }
}
