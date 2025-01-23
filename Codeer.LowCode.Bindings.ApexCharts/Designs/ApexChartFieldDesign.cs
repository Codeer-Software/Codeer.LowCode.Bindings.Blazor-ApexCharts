using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.DesignLogic.Check;
using Codeer.LowCode.Blazor.DesignLogic.Location;
using Codeer.LowCode.Blazor.DesignLogic.Refactor;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Designs
{
    public class ApexChartFieldDesign() : ApexChartFieldDesignBase(typeof(ApexChartFieldDesign).FullName!)
    {
        public override SeriesType SeriesType { get; set; } = SeriesType.Bar;

        [Designer]
        public ChartSeries Series { get; set; } = new();

        [Designer(Category = "ApexCharts - Bar")]
        public bool FullWidthBar { get; set; }

        [Designer(Category = "ApexCharts - Grid", DisplayName = "Show X-axis Grid")]
        public bool ShowXAxisGrid { get; set; }

        [Designer(Category = "ApexCharts - Grid", DisplayName = "Show Y-axis Grid")]
        public bool ShowYAxisGrid { get; set; } = true;

        public override List<DesignCheckInfo> CheckDesign(DesignCheckContext context)
        {
            var result = base.CheckDesign(context);
            foreach (var s in Series.Series)
            {
                context.CheckFieldRelativeFieldExistence(Name, nameof(s.Name), SearchCondition.ModuleName,
                        s.Name)
                    .AddTo(result);
            }

            var hasHeatmap = Series.Series.Any(s => s.Type == SeriesType.Heatmap);
            if (hasHeatmap && Series.Series.Any(s => s.Type != SeriesType.Heatmap))
            {
                result.Add(new FieldDesignCheckInfo()
                {
                    Location = new FieldDesignDataLocation()
                    {
                        Module = context.OwnerModule,
                        Field = Name,
                        Member = nameof(Series)
                    },
                    Message = "Heatmap series and non-heatmap series cannot be mixed."
                });
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
