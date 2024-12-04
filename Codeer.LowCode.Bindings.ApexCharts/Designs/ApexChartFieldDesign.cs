using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Components;
using Codeer.LowCode.Bindings.ApexCharts.Fields;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.DesignLogic.Check;
using Codeer.LowCode.Blazor.DesignLogic.Refactor;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;

namespace Codeer.LowCode.Bindings.ApexCharts.Designs
{
    public class ApexChartFieldDesign() : FieldDesignBase(typeof(ApexChartFieldDesign).FullName!), IDisplayName,
        ISearchResultsViewFieldDesign
    {
        [Designer(Scope = DesignerScope.All)]
        public SearchCondition SearchCondition { get; set; } = new();

        [Designer]
        public string DisplayName { get; set; } = string.Empty;

        [Designer]
        public SeriesType SeriesType { get; set; } = SeriesType.Bar;

        [Designer(CandidateType = CandidateType.Field)]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        public string? CategoryField { get; set; }

        [Designer]
        public string? CategoryFormat { get; set; }

        [Designer(CandidateType = CandidateType.Field)]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        [TargetFieldType(Types = [typeof(NumberFieldDesign)])]
        public List<string> SeriesFields { get; set; } = [];

        [Designer]
        public int SeriesFractionDigits { get; set; } = 2;

        [Designer]
        public bool ShowLegend { get; set; } = true;

        public override string GetWebComponentTypeFullName() => typeof(ApexChartFieldComponent).FullName!;

        public override string GetSearchWebComponentTypeFullName() => string.Empty;

        public override string GetSearchControlTypeFullName() => string.Empty;

        public override FieldBase CreateField() => new ApexChartField(this);

        public override FieldDataBase? CreateData() => null;

        public override List<DesignCheckInfo> CheckDesign(DesignCheckContext context)
        {
            var result = base.CheckDesign(context);
            context.CheckFieldRelativeFieldExistence(Name, nameof(CategoryField), SearchCondition.ModuleName, CategoryField ?? "").AddTo(result);
            foreach (var s in SeriesFields)
            {
                context.CheckFieldRelativeFieldExistence(Name, nameof(SeriesFields), SearchCondition.ModuleName, s).AddTo(result);
            }
            result.AddRange(SearchCondition.CheckDesign(context, Name, nameof(SearchCondition)));
            return result;
        }

        public override RenameResult ChangeName(RenameContext context)
        {
            var builder = context.Builder(base.ChangeName(context))
            .AddMatchCondition(SearchCondition);
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
