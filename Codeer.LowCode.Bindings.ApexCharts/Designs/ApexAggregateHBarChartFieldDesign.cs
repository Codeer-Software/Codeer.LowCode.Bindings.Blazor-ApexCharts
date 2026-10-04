using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.DesignLogic.Refactor;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Designs
{
    /// <summary>集計横棒チャート (系列は棒に固定)。ランキング (値の大きい順 + 上限) に向く。</summary>
    [Designer(DisplayName = "$ApexAggregateHBarChartField")]
    public class ApexAggregateHBarChartFieldDesign() : ApexAggregateChartFieldDesignBase(typeof(ApexAggregateHBarChartFieldDesign).FullName!)
    {
        [Designer(Index = 10, DisplayName = "$Series")]
        public AggregateChartSeries Series { get; set; } = new();

        [Designer(Index = 6, Category = "$CategoryAggregate", CandidateType = CandidateType.Field, DisplayName = "$SeriesGroupField")]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        public string SeriesGroupField { get; set; } = string.Empty;

        [Designer(Index = 21, DisplayName = "$SeriesFractionDigits")]
        public int SeriesFractionDigits { get; set; } = 2;

        //横棒は種類を選べない (棒に揃える)
        internal override IReadOnlyList<AggregateSeries> GetSeries()
            => Series.Series.Select(s => new AggregateSeries { Function = s.Function, Name = s.Name, Title = s.Title, Color = s.Color, Type = SeriesType.Bar }).ToList();
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
