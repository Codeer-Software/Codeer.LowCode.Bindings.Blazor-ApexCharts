using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.DesignLogic.Check;
using Codeer.LowCode.Blazor.DesignLogic.Refactor;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Designs
{
    public class ApexChartFieldDesign() : ApexChartFieldDesignBase(typeof(ApexChartFieldDesign).FullName!)
    {
        public override SeriesType SeriesType { get; set; } = SeriesType.Bar;

        [Designer]
        public ChartSeries Series { get; set; } = new();

        [Designer(Category = "ApexChart - Bar")]
        public bool FullWidthBar { get; set; }

        public override List<DesignCheckInfo> CheckDesign(DesignCheckContext context)
        {
            var result = base.CheckDesign(context);
            foreach (var s in Series.Series)
            {
                context.CheckFieldRelativeFieldExistence(Name, nameof(s.Name), SearchCondition.ModuleName,
                        s.Name)
                    .AddTo(result);
            }

            return result;
        }

        public override RenameResult ChangeName(RenameContext context)
        {
            var builder = context.Builder(base.ChangeName(context));
            if (CategoryField != null) builder.AddField(SearchCondition.ModuleName, CategoryField, s => CategoryField = s);
            for (var i = 0; i < Series.Series.Count; ++i)
            {
                var index = i;
                Action<string> change = s => Series.Series[index].Name = s;
                builder.AddField(SearchCondition.ModuleName, Series.Series[index].Name, change);
            }

            return builder.Build();
        }
    }
}
