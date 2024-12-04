using ApexCharts;
using Codeer.LowCode.Blazor.DesignLogic.Check;
using Codeer.LowCode.Blazor.DesignLogic.Refactor;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Designs
{
    public class ApexRadialChartFieldDesign() : ApexChartFieldDesignBase(typeof(ApexRadialChartFieldDesign).FullName!)
    {
        [Designer]
        [EnumIgnore(SeriesType.Treemap)]
        [EnumIgnore(SeriesType.RangeArea)]
        [EnumIgnore(SeriesType.Radar)]
        [EnumIgnore(SeriesType.RadialBar)]
        [EnumIgnore(SeriesType.Area)]
        [EnumIgnore(SeriesType.Bar)]
        [EnumIgnore(SeriesType.Heatmap)]
        [EnumIgnore(SeriesType.Line)]
        [EnumIgnore(SeriesType.Scatter)]
        public override SeriesType SeriesType { get; set; } = SeriesType.Bar;

        [Designer(CandidateType = CandidateType.Field)]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        [TargetFieldType(Types = [typeof(NumberFieldDesign)])]
        public string? SeriesField { get; set; }

        public override List<DesignCheckInfo> CheckDesign(DesignCheckContext context)
        {
            var result = base.CheckDesign(context);
            context.CheckFieldRelativeFieldExistence(Name, nameof(SeriesField), SearchCondition.ModuleName, SeriesField ?? "").AddTo(result);
            return result;
        }

        public override RenameResult ChangeName(RenameContext context)
        {
            var builder = context.Builder(base.ChangeName(context));
            if (SeriesField != null) builder.AddField(SeriesField, s => SeriesField = s);
            return builder.Build();
        }
    }
}
