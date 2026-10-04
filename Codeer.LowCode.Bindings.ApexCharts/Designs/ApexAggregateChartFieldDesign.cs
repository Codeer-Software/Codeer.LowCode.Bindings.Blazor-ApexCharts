using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.DesignLogic.Refactor;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Designs
{
    /// <summary>集計チャート (棒・折れ線・面・ヒートマップ)。系列ごとに集計方法と種類を選べ、系列を分ける項目の値ごとに系列を作れる。</summary>
    [Designer(DisplayName = "$ApexAggregateChartField")]
    public class ApexAggregateChartFieldDesign() : ApexAggregateChartFieldDesignBase(typeof(ApexAggregateChartFieldDesign).FullName!)
    {
        [Designer(Index = 10, DisplayName = "$Series")]
        public AggregateChartSeries Series { get; set; } = new();

        [Designer(Index = 6, Category = "$CategoryAggregate", CandidateType = CandidateType.Field, DisplayName = "$SeriesGroupField")]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        public string SeriesGroupField { get; set; } = string.Empty;

        [Designer(Index = 21, DisplayName = "$SeriesFractionDigits")]
        public int SeriesFractionDigits { get; set; } = 2;

        [Designer(Index = 22, Category = "$CategoryGrid", DisplayName = "$ShowXAxisGrid")]
        public bool ShowXAxisGrid { get; set; }

        [Designer(Index = 23, Category = "$CategoryGrid", DisplayName = "$ShowYAxisGrid")]
        public bool ShowYAxisGrid { get; set; } = true;

        internal override IReadOnlyList<AggregateSeries> GetSeries() => Series.Series;
        internal override string GetSeriesGroupField() => SeriesGroupField;
        internal override int? GetFractionDigits() => SeriesFractionDigits;

        internal override void AddRenameTargets(RenameContext.RenameResultBuilder builder, string moduleName)
        {
            if (!string.IsNullOrEmpty(SeriesGroupField)) builder.AddField(moduleName, SeriesGroupField, s => SeriesGroupField = s);
            foreach (var series in Series.Series.Where(s => !string.IsNullOrEmpty(s.Name)))
            {
                var target = series;
                builder.AddField(moduleName, target.Name, s => target.Name = s);
            }
        }
    }
}
