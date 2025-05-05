using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Designs;

namespace Codeer.LowCode.Bindings.ApexCharts
{
    public static class ApexChartsServerInitializer
    {
        public static void Initialize()
        {
            //load dll.
            typeof(ApexChartFieldDesign).ToString();
            typeof(SeriesType).ToString();
        }
    }
}
