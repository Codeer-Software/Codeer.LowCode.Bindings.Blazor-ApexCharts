using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.DesignLogic.Refactor;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Designs
{
    /// <summary>集計円チャート (円・ドーナツ・ポーラー)。カテゴリごとの 1 つの集計値を割合で見せる。</summary>
    [Designer(DisplayName = "$ApexAggregateRadialChartField")]
    public class ApexAggregateRadialChartFieldDesign() : ApexAggregateChartFieldDesignBase(typeof(ApexAggregateRadialChartFieldDesign).FullName!)
    {
        [Designer(Index = 9, DisplayName = "$SeriesType")]
        [EnumIgnore(SeriesType.Treemap)]
        [EnumIgnore(SeriesType.RangeArea)]
        [EnumIgnore(SeriesType.Radar)]
        [EnumIgnore(SeriesType.RadialBar)]
        [EnumIgnore(SeriesType.Area)]
        [EnumIgnore(SeriesType.Bar)]
        [EnumIgnore(SeriesType.Heatmap)]
        [EnumIgnore(SeriesType.Line)]
        [EnumIgnore(SeriesType.Scatter)]
        public SeriesType SeriesType { get; set; } = SeriesType.Pie;

        [Designer(Index = 10, DisplayName = "$SeriesFunction")]
        public ChartAggregateFunction SeriesFunction { get; set; } = ChartAggregateFunction.Count;

        [Designer(Index = 11, CandidateType = CandidateType.Field, DisplayName = "$SeriesField")]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        [TargetFieldType(Types = [typeof(NumberFieldDesign)])]
        public string SeriesField { get; set; } = string.Empty;

        internal override IReadOnlyList<AggregateSeries> GetSeries()
            => [new AggregateSeries { Function = SeriesFunction, Name = SeriesFunction == ChartAggregateFunction.Count ? string.Empty : SeriesField, Type = SeriesType }];

        internal override void AddRenameTargets(RenameContext.RenameResultBuilder builder, string moduleName)
        {
            if (!string.IsNullOrEmpty(SeriesField)) builder.AddField(moduleName, SeriesField, s => SeriesField = s);
        }
    }
}
