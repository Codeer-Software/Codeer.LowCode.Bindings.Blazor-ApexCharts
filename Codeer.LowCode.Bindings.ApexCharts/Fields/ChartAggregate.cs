using System.Globalization;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.Aggregation;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;

namespace Codeer.LowCode.Bindings.ApexCharts.Fields
{
    /// <summary>集計チャートのグラフに描く形 (系列と、カテゴリごとの値)。系列の Name は内部の鍵 (s0, s1...) で、見出しは Titles に持つ。</summary>
    internal class AggregateChartData
    {
        public List<Series> Series { get; } = [];
        public List<SeriesData> Data { get; } = [];
        public Dictionary<string, string> Titles { get; } = [];
    }

    /// <summary>
    /// 集計チャートの変換 (画面に依存しない部分)。設計 → 集計定義、クロス表 → グラフの系列とカテゴリの値、軸の値の見出し。
    /// カテゴリはクロス表の行 (軸の先頭 categoryCount 個)、系列を分ける項目は列で、系列は 値 × 列の値 で作る。
    /// </summary>
    internal static class ChartAggregate
    {
        internal static readonly string[] DefaultTheme = ["#008FFB", "#00E396", "#FEB019", "#FF4560", "#775DD0"];

        internal static AggregateFunction ToFunction(ChartAggregateFunction function) => function switch
        {
            ChartAggregateFunction.Count => AggregateFunction.Count,
            ChartAggregateFunction.Avg => AggregateFunction.Avg,
            ChartAggregateFunction.Min => AggregateFunction.Min,
            ChartAggregateFunction.Max => AggregateFunction.Max,
            _ => AggregateFunction.Sum,
        };

        internal static DateBucket? ToBucket(ChartDateUnit unit) => unit switch
        {
            ChartDateUnit.Year => DateBucket.Year,
            ChartDateUnit.Quarter => DateBucket.Quarter,
            ChartDateUnit.Month => DateBucket.Month,
            ChartDateUnit.Week => DateBucket.Week,
            ChartDateUnit.Day => DateBucket.Day,
            ChartDateUnit.Hour => DateBucket.Hour,
            _ => null,
        };

        static string Variable(string fieldName) => fieldName + ".Value";

        /// <summary>設計を集計定義にする (条件は SearchCondition の条件だけ。並び・件数上限は使わない)。カテゴリは先頭の 1 軸。</summary>
        internal static AggregateCondition CreateCondition(ApexAggregateChartFieldDesignBase design)
        {
            var condition = new AggregateCondition(design.SearchCondition.ModuleName) { Condition = design.SearchCondition.Condition };
            var bucket = ToBucket(design.CategoryDateUnit);
            condition.Groups.Add(bucket == null
                ? new ValueGroup { Variable = Variable(design.CategoryField) }
                : new DateGroup { Variable = Variable(design.CategoryField), Bucket = bucket.Value, FiscalYearStartMonth = design.FiscalYearStartMonth is >= 1 and <= 12 ? design.FiscalYearStartMonth : 1 });
            var groupField = design.GetSeriesGroupField();
            if (!string.IsNullOrEmpty(groupField)) condition.Groups.Add(new ValueGroup { Variable = Variable(groupField) });
            foreach (var s in design.GetSeries())
            {
                var function = ToFunction(s.Function);
                condition.Measures.Add(new AggregateMeasure
                {
                    Function = function,
                    Variable = function == AggregateFunction.Count ? string.Empty : Variable(s.Name),
                    Name = s.Title,
                });
            }
            if (design.CategoryOrder != ChartCategoryOrder.Field && condition.Measures.Count > 0)
                condition.SortConditions.Add(new AggregateSort { Target = AggregateSortTarget.Measure, Index = 0, IsDescending = design.CategoryOrder == ChartCategoryOrder.ValueDescending });
            if (design.CategoryLimit > 0) condition.LimitCount = design.CategoryLimit;
            return condition;
        }

        /// <summary>
        /// 設計の不整合 (日付でない項目にまとめる単位・項目に使えない集計方法・ヒートマップの混在) を (番号, メンバー, 文言) で返す。
        /// 項目が無い指摘は本体の存在確認が出すので、ここでは見つかった項目だけを見る。
        /// </summary>
        internal static List<(int Code, string Member, string Message)> Validate(ApexAggregateChartFieldDesignBase design, ModuleDesign module)
        {
            var result = new List<(int, string, string)>();
            var bucket = ToBucket(design.CategoryDateUnit);
            var category = module.Fields.FirstOrDefault(e => e.Name == design.CategoryField);
            if (bucket != null && category != null && !new DateGroup { Bucket = bucket.Value }.CanApplyTo(category))
                result.Add((ApexAggregateChartFieldDesignBase.Codes.DateUnitRequiresDate, nameof(design.CategoryDateUnit), string.Format(Properties.Resources.Check_DateUnitRequiresDate, design.CategoryField)));

            var series = design.GetSeries();
            foreach (var s in series.Where(s => s.Function != ChartAggregateFunction.Count))
            {
                var field = module.Fields.FirstOrDefault(e => e.Name == s.Name);
                if (field == null) continue;
                //グラフの値は数値なので、最小・最大も数値の項目だけ
                var measure = new AggregateMeasure { Function = ToFunction(s.Function), Variable = Variable(s.Name) };
                if (!measure.CanApplyTo(field) || field is not NumberFieldDesign)
                    result.Add((ApexAggregateChartFieldDesignBase.Codes.InvalidFunction, "Series", string.Format(Properties.Resources.Check_InvalidFunction, s.Function.GetDisplayName(), s.Name)));
            }
            if (series.Any(s => s.Type == SeriesType.Heatmap) && series.Any(s => s.Type != SeriesType.Heatmap))
                result.Add((ApexAggregateChartFieldDesignBase.Codes.HeatmapCannotBeMixed, "Series", Properties.Resources.Check_HeatmapCannotBeMixed));
            return result;
        }

        /// <summary>系列の見た目 (設計の系列の同じ番号。無ければ棒・色なし)。</summary>
        internal record SeriesStyle(SeriesType Type, string Color);

        /// <summary>
        /// クロス表をグラフの形にする。カテゴリ = 表の行、系列 = 値 × 表の列 (列が無ければ値ごとに 1 系列)。
        /// 色: 列で分けないときは styles の色 (空なら既定の色を順に)、分けたときは既定の色を順に。種類は値の styles の種類。
        /// keyText は軸の値の見出し (index は表の行・列の中での軸の番号)、measureTitle は値の見出し。
        /// </summary>
        internal static AggregateChartData ToChart(CrossTab table, IReadOnlyList<SeriesStyle> styles,
            Func<AggregateKey, int, bool, string> keyText, Func<AggregateMeasure, string> measureTitle)
        {
            var chart = new AggregateChartData();
            var split = table.Columns.Any(c => c.Keys.Count > 0);
            string AxisText(CrossTabAxisValue axis, bool isCategory) => string.Join(" / ", axis.Keys.Select((k, i) => keyText(k, i, isCategory)));

            var keys = new List<(string Key, int Measure, int Column)>();
            for (var m = 0; m < table.Measures.Count; m++)
            {
                var style = m < styles.Count ? styles[m] : new SeriesStyle(SeriesType.Bar, string.Empty);
                for (var c = 0; c < table.Columns.Count; c++)
                {
                    var index = keys.Count;
                    var key = "s" + index.ToString(CultureInfo.InvariantCulture);
                    var color = !split && !string.IsNullOrEmpty(style.Color) ? style.Color : DefaultTheme[index % DefaultTheme.Length];
                    var title = !split ? measureTitle(table.Measures[m])
                        : table.Measures.Count == 1 ? AxisText(table.Columns[c], false)
                        : $"{AxisText(table.Columns[c], false)} / {measureTitle(table.Measures[m])}";
                    chart.Series.Add(new Series { Name = key, Color = color, Type = style.Type });
                    chart.Titles[key] = title;
                    keys.Add((key, m, c));
                }
            }
            for (var r = 0; r < table.Rows.Count; r++)
            {
                var data = new SeriesData { XValue = AxisText(table.Rows[r], true) };
                foreach (var (key, m, c) in keys) data.Data[key] = table.Cells[m][r][c].GetValue() as decimal?;
                chart.Data.Add(data);
            }
            return chart;
        }

        /// <summary>
        /// 軸の値の見出し。表示名 (選択の候補名・リンク先の表示項目) があればそれ、空値は emptyText。
        /// 日付は まとめる単位 に合わせ (年 yyyy・四半期 yyyy Qn (年度の開始年)・月 yyyy-MM・週と日 yyyy-MM-dd・時 yyyy-MM-dd HH:00)、真偽は項目の文言、UTC で保存した日時はローカル時刻。
        /// </summary>
        internal static string KeyText(AggregateKey key, AggregateGroup? group, FieldDesignBase? field, string emptyText, CultureInfo culture)
        {
            if (!string.IsNullOrEmpty(key.DisplayText)) return key.DisplayText;
            var value = key.Value.GetValue();
            if (value == null) return emptyText;
            if (group is DateGroup dateGroup)
            {
                switch (value)
                {
                    case DateOnly date:
                        return dateGroup.Bucket switch
                        {
                            DateBucket.Year => date.Year.ToString(CultureInfo.InvariantCulture),
                            DateBucket.Quarter => QuarterText(date, dateGroup.FiscalYearStartMonth),
                            DateBucket.Month => date.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                            _ => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        };
                    //時でまとめた鍵は SQL が時差を足して丸め済
                    case DateTime hour: return hour.ToString("yyyy-MM-dd HH:00", CultureInfo.InvariantCulture);
                }
            }
            return value switch
            {
                DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                DateTime t => (field is DateTimeFieldDesign { SaveAsUtc: true } ? DateTime.SpecifyKind(t, DateTimeKind.Utc).ToLocalTime() : t).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                TimeOnly t => t.ToString("HH:mm", CultureInfo.InvariantCulture),
                bool b => BoolText(b, field as BooleanFieldDesign),
                decimal m => m.ToString(culture),
                _ => Convert.ToString(value, culture) ?? string.Empty,
            };
        }

        //四半期の鍵は期間の開始日。年度の開始月から数えた番号と、その年度の開始年で出す
        static string QuarterText(DateOnly start, int fiscalYearStartMonth)
        {
            var startMonth = fiscalYearStartMonth is >= 1 and <= 12 ? fiscalYearStartMonth : 1;
            var offset = (start.Month - startMonth + 12) % 12;
            var year = start.Month >= startMonth ? start.Year : start.Year - 1;
            return $"{year.ToString(CultureInfo.InvariantCulture)} Q{offset / 3 + 1}";
        }

        static string BoolText(bool value, BooleanFieldDesign? field)
        {
            var text = value ? field?.TrueText : field?.FalseText;
            return string.IsNullOrEmpty(text) ? value.ToString() : text;
        }

        /// <summary>変数名から元モジュールの項目を引く。リンク越し ("Customer.Region.Value") はリンク (LinkField / ModuleField / モジュール参照の SelectField) をたどる。無ければ null。</summary>
        internal static FieldDesignBase? ResolveField(DesignData designData, ModuleDesign module, string variable)
        {
            if (string.IsNullOrEmpty(variable)) return null;
            var fieldName = new VariableName(variable).FieldName;
            var current = module;
            while (true)
            {
                var exact = current.Fields.FirstOrDefault(e => e.Name == fieldName.FullName);
                if (exact != null || !fieldName.IsLink) return exact;
                var link = current.Fields.FirstOrDefault(e => e.Name == fieldName.Root);
                var target = link switch
                {
                    LinkFieldDesign l => l.SearchCondition.ModuleName,
                    ModuleFieldDesign m => m.ModuleName,
                    SelectFieldDesign s when !string.IsNullOrEmpty(s.SearchCondition.ModuleName) => s.SearchCondition.ModuleName,
                    _ => null,
                };
                if (string.IsNullOrEmpty(target) || designData.Modules.Find(target) is not { } next) return null;
                current = next;
                fieldName = fieldName.SkipRoot();
            }
        }
    }
}
