using Codeer.LowCode.Bindings.ApexCharts.Components;
using Codeer.LowCode.Bindings.ApexCharts.Fields;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.DesignLogic.Check;
using Codeer.LowCode.Blazor.DesignLogic.Location;
using Codeer.LowCode.Blazor.DesignLogic.Refactor;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;

namespace Codeer.LowCode.Bindings.ApexCharts.Designs
{
    /// <summary>
    /// 集計チャート 3 種の共通部分。元モジュール (SearchCondition) の行を、カテゴリの項目でまとめて集計し (DB 側で GROUP BY。権限は一覧と同じ)、グラフにする。
    /// 元モジュールを空にしておき、スクリプトの Show(ModuleAggregator) で集計定義を渡して表示することもできる。
    /// </summary>
    [IgnoreBaseProperties(nameof(IgnoreModification), nameof(OnValidateInput))]
    public abstract class ApexAggregateChartFieldDesignBase(string fullName) : FieldDesignBase(fullName), IDisplayName, ISearchResultsViewFieldDesign
    {
        /// <summary>デザインチェック指摘の番号 (固定。追加は末尾・欠番は再利用しない)。</summary>
        public static class Codes
        {
            public const int NoCategory = 1;
            public const int DateUnitRequiresDate = 2;
            public const int InvalidFunction = 3;
            public const int HeatmapCannotBeMixed = 4;
            public const int NoSeries = 5;
            public const int NotTableModule = 6;
        }

        [Designer(Index = 1, Scope = DesignerScope.All, DisplayName = "$SearchCondition")]
        public SearchCondition SearchCondition { get; set; } = new();

        [Designer(Index = 2, DisplayName = "$DisplayName")]
        public string DisplayName { get; set; } = string.Empty;

        [Designer(Index = 3, CandidateType = CandidateType.Field, DisplayName = "$CategoryField")]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        public string CategoryField { get; set; } = string.Empty;

        [Designer(Index = 4, Category = "$CategoryAggregate", DisplayName = "$CategoryDateUnit")]
        public ChartDateUnit CategoryDateUnit { get; set; } = ChartDateUnit.None;

        [Designer(Index = 5, Category = "$CategoryAggregate", DisplayName = "$FiscalYearStartMonth")]
        public int FiscalYearStartMonth { get; set; } = 1;

        [Designer(Index = 7, Category = "$CategoryAggregate", DisplayName = "$CategoryOrder")]
        public ChartCategoryOrder CategoryOrder { get; set; } = ChartCategoryOrder.Field;

        [Designer(Index = 8, Category = "$CategoryAggregate", DisplayName = "$CategoryLimit")]
        public int CategoryLimit { get; set; }

        [Designer(Index = 20, DisplayName = "$ShowLegend")]
        public bool ShowLegend { get; set; } = true;

        /// <summary>閲覧者が画面で集計 (カテゴリ・系列・並び等) を変更できる。変更はブラウザに保存され、その人の画面だけに効く。</summary>
        [Designer(Index = 30, Scope = DesignerScope.All, DisplayName = "$CanCustomize")]
        public bool CanCustomize { get; set; }

        public override string GetWebComponentTypeFullName() => typeof(ApexAggregateChartFieldComponent).FullName!;
        public override string GetSearchWebComponentTypeFullName() => string.Empty;
        public override string GetSearchControlTypeFullName() => string.Empty;
        public override FieldBase CreateField() => new ApexAggregateChartField(this);
        public override FieldDataBase? CreateData() => null;

        /// <summary>設計の系列 (集計チャート・横棒は一覧、円は 1 つ)。</summary>
        internal abstract IReadOnlyList<AggregateSeries> GetSeries();

        /// <summary>系列を分ける項目 (円は分けないので空)。</summary>
        internal virtual string GetSeriesGroupField() => string.Empty;

        /// <summary>設計の集計の設定 (カテゴリ・系列・並び等)。</summary>
        internal AggregateChartSetting GetSetting() => new()
        {
            CategoryField = CategoryField,
            CategoryDateUnit = CategoryDateUnit,
            FiscalYearStartMonth = FiscalYearStartMonth,
            SeriesGroupField = GetSeriesGroupField(),
            Series = GetSeries().Select(CloneSeries).ToList(),
            CategoryOrder = CategoryOrder,
            CategoryLimit = CategoryLimit,
        };

        /// <summary>利用者の設定をこのチャートで描ける形に揃える (横棒は棒に固定・円は値 1 つで分けない)。設計の GetSeries と同じ揃え方。</summary>
        internal virtual AggregateChartSetting NormalizeSetting(AggregateChartSetting setting) => setting;

        internal static AggregateSeries CloneSeries(AggregateSeries s)
            => new() { Function = s.Function, Name = s.Name, Title = s.Title, Type = s.Type, Color = s.Color };

        /// <summary>値の軸の小数桁 (軸の無い円は null)。</summary>
        internal virtual int? GetFractionDigits() => null;

        public override List<DesignCheckInfo> CheckDesign(DesignCheckContext context)
        {
            var result = base.CheckDesign(context);
            result.AddRange(SearchCondition.CheckDesign(context, Name, nameof(SearchCondition)));
            //元モジュールが空ならスクリプトの Show 専用 (設定は使わない)
            var moduleName = SearchCondition.ModuleName;
            if (string.IsNullOrEmpty(moduleName)) return result;

            if (string.IsNullOrEmpty(CategoryField))
            {
                result.Add(Info(context, Codes.NoCategory, nameof(CategoryField), Properties.Resources.Check_NoCategory));
            }
            else
            {
                context.CheckFieldRelativeFieldExistence(Name, nameof(CategoryField), moduleName, CategoryField).AddTo(result);
            }
            var groupField = GetSeriesGroupField();
            if (!string.IsNullOrEmpty(groupField)) context.CheckFieldRelativeFieldExistence(Name, "SeriesGroupField", moduleName, groupField).AddTo(result);

            var series = GetSeries();
            if (series.Count == 0) result.Add(Info(context, Codes.NoSeries, "Series", Properties.Resources.Check_NoSeries));
            foreach (var s in series.Where(s => s.Function != ChartAggregateFunction.Count))
                context.CheckFieldRelativeFieldExistence(Name, "Series", moduleName, s.Name).AddTo(result);

            var module = context.DesignData.Modules.Find(moduleName);
            if (module != null)
            {
                //集計はテーブルを持つモジュールだけ (サーバーが拒否する)。モジュールが無いことは SearchCondition の確認が出す
                if (module.Fields.OfType<QueryFieldDesign>().Any() || string.IsNullOrEmpty(module.DbTable))
                    result.Add(Info(context, Codes.NotTableModule, nameof(SearchCondition), string.Format(Properties.Resources.Check_NotTableModule, moduleName)));
                foreach (var (code, member, message) in ChartAggregate.Validate(GetSetting(), module))
                    result.Add(Info(context, code, member, message));
            }
            return result;
        }

        FieldDesignCheckInfo Info(DesignCheckContext context, int code, string member, string message) => new()
        {
            Code = DesignCheckCode.Create(typeof(ApexAggregateChartFieldDesignBase), code),
            Location = new FieldDesignDataLocation { Module = context.OwnerModule, Field = Name, Member = member },
            Message = message,
        };

        public override RenameResult ChangeName(RenameContext context)
        {
            var builder = context.Builder(base.ChangeName(context)).AddMatchCondition(SearchCondition);
            var moduleName = SearchCondition.ModuleName;
            if (!string.IsNullOrEmpty(CategoryField)) builder.AddField(moduleName, CategoryField, s => CategoryField = s);
            AddRenameTargets(builder, moduleName);
            return builder.Build();
        }

        /// <summary>派生の項目 (系列・系列を分ける項目) を改名の対象に足す。</summary>
        internal abstract void AddRenameTargets(RenameContext.RenameResultBuilder builder, string moduleName);
    }
}
