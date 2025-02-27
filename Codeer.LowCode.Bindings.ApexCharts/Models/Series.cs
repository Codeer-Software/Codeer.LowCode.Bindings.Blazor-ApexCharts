using ApexCharts;

namespace Codeer.LowCode.Bindings.ApexCharts.Models
{
    public class Series
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public SeriesType Type { get; set; } = SeriesType.Line;
    }
}
