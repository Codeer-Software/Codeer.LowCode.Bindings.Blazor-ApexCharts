using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Designer.Controls;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.Designer;

namespace Codeer.LowCode.Bindings.ApexCharts.Designer
{
    public static class ApexChartsDesignerInitializer
    {
        public static void Initialize()
        {
            //load dll.
            typeof(ApexChartFieldDesign).ToString();
            typeof(SeriesType).ToString();
            DesignerApp.ScriptRuntimeTypeManager.AddType<AnnotationAxis>();
            DesignerApp.ScriptRuntimeTypeManager.AddType<ChartAnnotation>();
            PropertyTypeManager.AddPropertyControl<ChartSeries, ChartSeriesPropertyControl>();
        }
    }
}
