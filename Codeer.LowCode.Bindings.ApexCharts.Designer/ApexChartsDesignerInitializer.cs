using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Designer.Controls;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.Designer;
using Codeer.LowCode.Blazor.Designer.Extensibility;

namespace Codeer.LowCode.Bindings.ApexCharts.Designer
{
    public static class ApexChartsDesignerInitializer
    {
        public static void Initialize(BlazorRuntime blazorRuntime)
        {
            InitializeCore();
            blazorRuntime.InstallBundleCss("Codeer.LowCode.Bindings.ApexCharts");
        }

        [Obsolete("Use Initialize(BlazorRuntime) instead. Without it, scoped CSS for ApexCharts components is not installed.")]
        public static void Initialize() => InitializeCore();

        static void InitializeCore()
        {
            //load dll.
            typeof(ApexChartFieldDesign).ToString();
            typeof(SeriesType).ToString();
            DesignerApp.ScriptRuntimeTypeManager.AddType<AnnotationAxis>();
            DesignerApp.ScriptRuntimeTypeManager.AddType<ChartAnnotation>();
            PropertyTypeManager.AddPropertyControl<ChartSeries, ChartSeriesPropertyControl>();

            //AI 用フィールドドキュメント。.md は本体プロジェクトにあるが、WASM に配信しないため
            //このアセンブリに埋め込まれている (リンク EmbeddedResource)。FieldCatalog が
            //`.FieldDocs.<型名>.md` 規約で解決できるよう、探索先として登録する。
            FieldCatalog.AddDocAssembly(typeof(ApexChartsDesignerInitializer).Assembly);
        }
    }
}
