using ApexCharts;
using Codeer.LowCode.Blazor.DesignLogic.Check;
using Codeer.LowCode.Blazor.DesignLogic.Refactor;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Designs
{
    public class ApexChartFieldDesign() : ApexChartFieldDesignBase(typeof(ApexChartFieldDesign).FullName!)
    {
        [Designer]
        [EnumIgnore(SeriesType.Treemap)]
        [EnumIgnore(SeriesType.RangeArea)]
        [EnumIgnore(SeriesType.Radar)]
        [EnumIgnore(SeriesType.RadialBar)]
        [EnumIgnore(SeriesType.Donut)]
        [EnumIgnore(SeriesType.Pie)]
        [EnumIgnore(SeriesType.PolarArea)]
        public override SeriesType SeriesType { get; set; } = SeriesType.Bar;

        [Designer(CandidateType = CandidateType.Field)]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        [TargetFieldType(Types = [typeof(NumberFieldDesign)])]
        public List<string> SeriesFields { get; set; } = [];

        [Designer(Category = "ApexChart - Bar")]
        public bool FullWidthBar { get; set; }

        public override List<DesignCheckInfo> CheckDesign(DesignCheckContext context)
        {
            var result = base.CheckDesign(context);
            foreach (var s in SeriesFields)
            {
                context.CheckFieldRelativeFieldExistence(Name, nameof(SeriesFields), SearchCondition.ModuleName, s).AddTo(result);
            }

            return result;
        }

        public override RenameResult ChangeName(RenameContext context)
        {
            var builder = context.Builder(base.ChangeName(context));
            if (CategoryField != null) builder.AddField(CategoryField, s => CategoryField = s);
            for (var i = 0; i < SeriesFields.Count; ++i)
            {
                Action<string> change = s => SeriesFields[i] = s;
                builder.AddField(SeriesFields[8], change);
            }

            return builder.Build();
        }
    }
}
