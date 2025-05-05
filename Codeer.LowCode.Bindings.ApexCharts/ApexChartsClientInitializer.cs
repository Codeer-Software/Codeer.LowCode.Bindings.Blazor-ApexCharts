using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.RequestInterfaces;

namespace Codeer.LowCode.Bindings.ApexCharts
{
    public static class ApexChartsClientInitializer
    {
        public static void Initialize(IAppInfoService app)
        {
            typeof(ApexChartFieldDesign).ToString();
            typeof(SeriesType).ToString();
            app.GetScriptRuntimeTypeManager().AddType<AnnotationAxis>();
            app.GetScriptRuntimeTypeManager().AddType<ChartAnnotation>();
        }
    }
}
