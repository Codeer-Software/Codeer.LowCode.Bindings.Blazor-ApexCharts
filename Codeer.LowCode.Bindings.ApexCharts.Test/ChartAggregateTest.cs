using System.Globalization;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Fields;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.Aggregation;
using Codeer.LowCode.Blazor.Repository;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;

namespace Codeer.LowCode.Bindings.ApexCharts.Test
{
    // 集計チャートの変換: 設計 → 集計定義 / クロス表 → 系列とカテゴリの値 / 軸の値の見出し / 設計の不整合
    public class ChartAggregateTest
    {
        static ApexAggregateChartFieldDesign Chart()
        {
            var design = new ApexAggregateChartFieldDesign { Name = "Chart", CategoryField = "SoldOn", CategoryDateUnit = ChartDateUnit.Quarter, FiscalYearStartMonth = 4, SeriesGroupField = "Status" };
            design.SearchCondition.ModuleName = "Sale";
            design.SearchCondition.Condition = new FieldValueMatchCondition { SearchTargetVariable = "Owner.Value", Comparison = MatchComparison.Equal, Value = MultiTypeValue.Create("A") };
            design.SearchCondition.LimitCount = 3;
            design.Series.Series.Add(new AggregateSeries { Function = ChartAggregateFunction.Sum, Name = "Amount", Title = "売上", Type = SeriesType.Bar });
            design.Series.Series.Add(new AggregateSeries { Function = ChartAggregateFunction.Count, Name = "Amount", Type = SeriesType.Line });
            return design;
        }

        [Test]
        public void 設計はカテゴリと系列を分ける項目の軸と系列ごとの値の集計定義になる()
        {
            var design = Chart();
            design.CategoryOrder = ChartCategoryOrder.ValueDescending;
            design.CategoryLimit = 5;
            var c = ChartAggregate.CreateCondition(design.SearchCondition, design.GetSetting());
            Assert.Multiple(() =>
            {
                Assert.That(c.ModuleName, Is.EqualTo("Sale"));
                Assert.That(c.Condition, Is.SameAs(design.SearchCondition.Condition));
                Assert.That(c.Groups, Has.Count.EqualTo(2));
                var date = (DateGroup)c.Groups[0];
                Assert.That((date.Variable, date.Bucket, date.FiscalYearStartMonth), Is.EqualTo(("SoldOn.Value", DateBucket.Quarter, 4)));
                Assert.That(c.Groups[1], Is.TypeOf<ValueGroup>().And.Property("Variable").EqualTo("Status.Value"));
                Assert.That(c.Measures.Select(m => (m.Function, m.Variable, m.Name)), Is.EqualTo(new[] { (AggregateFunction.Sum, "Amount.Value", "売上"), (AggregateFunction.Count, "", "") }));
                //並びは先頭の値の降順・上限はカテゴリの数。検索条件の件数上限は使わない
                Assert.That(c.SortConditions.Select(s => (s.Target, s.Index, s.IsDescending)), Is.EqualTo(new[] { (AggregateSortTarget.Measure, 0, true) }));
                Assert.That(c.LimitCount, Is.EqualTo(5));
            });
        }

        [Test]
        public void まとめない項目の順で上限なしなら値そのままの軸で並びも上限も付かない()
        {
            var design = Chart();
            design.CategoryDateUnit = ChartDateUnit.None;
            design.SeriesGroupField = string.Empty;
            design.FiscalYearStartMonth = 13;
            var c = ChartAggregate.CreateCondition(design.SearchCondition, design.GetSetting());
            Assert.That(c.Groups.Single(), Is.TypeOf<ValueGroup>());
            Assert.That(c.SortConditions, Is.Empty);
            Assert.That(c.LimitCount, Is.Null);

            //年度の開始月が範囲外なら暦年
            design.CategoryDateUnit = ChartDateUnit.Year;
            Assert.That(((DateGroup)ChartAggregate.CreateCondition(design.SearchCondition, design.GetSetting()).Groups[0]).FiscalYearStartMonth, Is.EqualTo(1));
        }

        [Test]
        public void 横棒は種類を棒に揃え円は件数なら項目なしの値1つで分けない()
        {
            var hbar = new ApexAggregateHBarChartFieldDesign { SeriesGroupField = "Status" };
            hbar.Series.Series.Add(new AggregateSeries { Name = "Amount", Type = SeriesType.Line });
            Assert.That(hbar.GetSeries().Single().Type, Is.EqualTo(SeriesType.Bar));
            Assert.That(hbar.GetSeriesGroupField(), Is.EqualTo("Status"));

            var radial = new ApexAggregateRadialChartFieldDesign { SeriesFunction = ChartAggregateFunction.Count, SeriesField = "Amount", SeriesType = SeriesType.Donut, CategoryField = "Status" };
            radial.SearchCondition.ModuleName = "Sale";
            var c = ChartAggregate.CreateCondition(radial.SearchCondition, radial.GetSetting());
            Assert.Multiple(() =>
            {
                Assert.That(c.Measures.Single().Function, Is.EqualTo(AggregateFunction.Count));
                Assert.That(c.Measures.Single().Variable, Is.Empty);
                Assert.That(c.Groups, Has.Count.EqualTo(1));
                Assert.That(radial.GetSeries().Single().Type, Is.EqualTo(SeriesType.Donut));
                Assert.That(radial.GetFractionDigits(), Is.Null);
            });
        }

        static AggregateKey Key(object? value, string text = "") => new() { Value = MultiTypeValue.Create(value), DisplayText = text };

        //行 = カテゴリ (2 つ)、列 = 系列を分ける値、値 = measures
        static CrossTab Table(List<CrossTabAxisValue> columns, params AggregateMeasure[] measures)
        {
            var rows = new List<CrossTabAxisValue> { new() { Keys = [Key("c1")] }, new() { Keys = [Key(null)] } };
            var table = new CrossTab { Rows = rows, Columns = columns, Measures = measures.ToList() };
            for (var m = 0; m < measures.Length; m++)
                table.Cells.Add(rows.Select((_, r) => columns.Select((_, c) => r == 1 && c == 0 ? (MultiTypeValue)new NullValue() : MultiTypeValue.Create((decimal)((m + 1) * 100 + r * 10 + c))).ToList()).ToList());
            return table;
        }

        static string KeyText(AggregateKey key, int index, bool isCategory) => key.Value.GetValue() == null ? "(空白)" : $"{(isCategory ? "C" : "S")}:{key.Value.GetValue()}";

        [Test]
        public void 分けない表は値ごとに1系列で色と種類は設計の系列の同じ番号()
        {
            var table = Table([new CrossTabAxisValue()], new AggregateMeasure { Function = AggregateFunction.Sum, Name = "売上" }, new AggregateMeasure { Function = AggregateFunction.Count });
            var chart = ChartAggregate.ToChart(table, [new(SeriesType.Bar, "#111111"), new(SeriesType.Line, "")], KeyText, m => string.IsNullOrEmpty(m.Name) ? m.Function.ToString() : m.Name);
            Assert.Multiple(() =>
            {
                Assert.That(chart.Series.Select(s => (s.Name, s.Type, s.Color)), Is.EqualTo(new[] { ("s0", SeriesType.Bar, "#111111"), ("s1", SeriesType.Line, ChartAggregate.DefaultTheme[1]) }));
                Assert.That(chart.Titles, Is.EqualTo(new Dictionary<string, string> { ["s0"] = "売上", ["s1"] = "Count" }));
                Assert.That(chart.Data.Select(d => d.XValue), Is.EqualTo(new[] { "C:c1", "(空白)" }));
                Assert.That(chart.Data[0].Data, Is.EqualTo(new Dictionary<string, decimal?> { ["s0"] = 100, ["s1"] = 200 }));
                //値の無いセルは null (線が途切れる)
                Assert.That(chart.Data[1].Data["s0"], Is.Null);
            });
        }

        [Test]
        public void 分けた表は値と列の値の組で系列を作り見出しは列の値で色は既定の色を順に()
        {
            var columns = new List<CrossTabAxisValue> { new() { Keys = [Key("10", "受付")] }, new() { Keys = [Key("30")] } };
            var one = ChartAggregate.ToChart(Table(columns, new AggregateMeasure { Function = AggregateFunction.Sum }), [new(SeriesType.Area, "#111111")], KeyText, _ => "金額 合計");
            Assert.Multiple(() =>
            {
                Assert.That(one.Series.Select(s => (s.Type, s.Color)), Is.EqualTo(new[] { (SeriesType.Area, ChartAggregate.DefaultTheme[0]), (SeriesType.Area, ChartAggregate.DefaultTheme[1]) }));
                //表示名の無い鍵も keyText に渡す (表示名の有無は keyText が決める)
                Assert.That(one.Titles.Values, Is.EqualTo(new[] { "S:10", "S:30" }));
                Assert.That(one.Data[0].Data, Is.EqualTo(new Dictionary<string, decimal?> { ["s0"] = 100, ["s1"] = 101 }));
            });

            //値が複数なら「列の値 / 値の見出し」
            var two = ChartAggregate.ToChart(Table(columns, new AggregateMeasure { Function = AggregateFunction.Sum }, new AggregateMeasure { Function = AggregateFunction.Count }),
                [], KeyText, m => m.Function.ToString());
            Assert.That(two.Titles.Values, Is.EqualTo(new[] { "S:10 / Sum", "S:30 / Sum", "S:10 / Count", "S:30 / Count" }));
            Assert.That(two.Series.Select(s => s.Type).Distinct(), Is.EqualTo(new[] { SeriesType.Bar }));
        }

        [Test]
        public void 軸の値の見出しは表示名_空白_日付の単位_真偽の文言_UTCはローカル()
        {
            var culture = CultureInfo.InvariantCulture;
            DateGroup Date(DateBucket bucket, int fy = 1) => new() { Bucket = bucket, FiscalYearStartMonth = fy };
            Assert.Multiple(() =>
            {
                Assert.That(ChartAggregate.KeyText(Key("10", "受付"), new ValueGroup(), null, "(空白)", culture), Is.EqualTo("受付"));
                Assert.That(ChartAggregate.KeyText(Key(null), new ValueGroup(), null, "(空白)", culture), Is.EqualTo("(空白)"));
                Assert.That(ChartAggregate.KeyText(Key(new DateOnly(2026, 4, 1)), Date(DateBucket.Year, 4), null, "", culture), Is.EqualTo("2026"));
                //年度 4 月始まりの 1 月始まりの四半期は前の年度の Q4
                Assert.That(ChartAggregate.KeyText(Key(new DateOnly(2026, 1, 1)), Date(DateBucket.Quarter, 4), null, "", culture), Is.EqualTo("2025 Q4"));
                Assert.That(ChartAggregate.KeyText(Key(new DateOnly(2026, 7, 1)), Date(DateBucket.Quarter), null, "", culture), Is.EqualTo("2026 Q3"));
                Assert.That(ChartAggregate.KeyText(Key(new DateOnly(2026, 2, 1)), Date(DateBucket.Month), null, "", culture), Is.EqualTo("2026-02"));
                Assert.That(ChartAggregate.KeyText(Key(new DateOnly(2026, 2, 2)), Date(DateBucket.Week), null, "", culture), Is.EqualTo("2026-02-02"));
                Assert.That(ChartAggregate.KeyText(Key(new DateTime(2026, 2, 2, 9, 0, 0)), Date(DateBucket.Hour), new DateTimeFieldDesign { SaveAsUtc = true }, "", culture), Is.EqualTo("2026-02-02 09:00"));
                Assert.That(ChartAggregate.KeyText(Key(true), new ValueGroup(), new BooleanFieldDesign { TrueText = "済" }, "", culture), Is.EqualTo("済"));
                Assert.That(ChartAggregate.KeyText(Key(false), new ValueGroup(), new BooleanFieldDesign(), "", culture), Is.EqualTo("False"));
                var utc = new DateTime(2026, 2, 2, 0, 30, 0);
                Assert.That(ChartAggregate.KeyText(Key(utc), new ValueGroup(), new DateTimeFieldDesign { SaveAsUtc = true }, "", culture),
                    Is.EqualTo(DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("yyyy-MM-dd HH:mm", culture)));
                Assert.That(ChartAggregate.KeyText(Key(utc), new ValueGroup(), new DateTimeFieldDesign(), "", culture), Is.EqualTo("2026-02-02 00:30"));
            });
        }

        static ModuleDesign Sale()
        {
            var module = new ModuleDesign { Name = "Sale" };
            module.Fields.Add(new TextFieldDesign { Name = "Title" });
            module.Fields.Add(new NumberFieldDesign { Name = "Amount" });
            module.Fields.Add(new DateFieldDesign { Name = "SoldOn" });
            module.Fields.Add(new TextFieldDesign { Name = "Status" });
            return module;
        }

        [Test]
        public void 不整合は日付でない項目のまとめる単位と数値でない項目の集計とヒートマップの混在()
        {
            var design = Chart();
            Assert.That(ChartAggregate.Validate(design.GetSetting(), Sale()), Is.Empty);

            design.CategoryField = "Title";
            design.Series.Series.Add(new AggregateSeries { Function = ChartAggregateFunction.Max, Name = "SoldOn", Type = SeriesType.Heatmap });
            //件数は項目を見ない・無い項目は本体の存在確認が出すのでここでは出さない
            design.Series.Series.Add(new AggregateSeries { Function = ChartAggregateFunction.Count, Name = "Title" });
            design.Series.Series.Add(new AggregateSeries { Function = ChartAggregateFunction.Sum, Name = "Nothing" });
            var codes = ChartAggregate.Validate(design.GetSetting(), Sale()).Select(e => e.Code).ToList();
            Assert.That(codes, Is.EqualTo(new[]
            {
                ApexAggregateChartFieldDesignBase.Codes.DateUnitRequiresDate,
                ApexAggregateChartFieldDesignBase.Codes.InvalidFunction,
                ApexAggregateChartFieldDesignBase.Codes.HeatmapCannotBeMixed,
            }));
        }
    }
}
