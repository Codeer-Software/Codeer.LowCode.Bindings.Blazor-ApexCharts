using ApexCharts;

namespace Codeer.LowCode.Bindings.ApexCharts.Models
{
    public class SeriesData
    {
        public object XValue { get; set; } = 0;
        public Dictionary<string, decimal?> Data { get; set; } = [];
    }
}
