using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Fields;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Aggregation;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.Script.Internal.ScriptServices;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codeer.LowCode.Bindings.ApexCharts.Test
{
    // 集計チャートの実行時: 設計の集計を 1 往復で取りグラフにする / 追加の条件は AND / Show の定義が優先 / 失敗は LoadError / 円は値 1 つ
    public class AggregateChartFieldTest
    {
        static DesignData Design(Action<ModuleDesign>? addField = null)
        {
            var d = new DesignData();
            var sale = new ModuleDesign { Name = "Sale", DataSourceName = "Main", DbTable = "sales" };
            sale.Fields.Add(new IdFieldDesign { Name = "Id", DbColumn = "id" });
            sale.Fields.Add(new NumberFieldDesign { Name = "Amount", DisplayName = "金額", DbColumn = "amount" });
            var status = new SelectFieldDesign { Name = "Status", DbColumn = "status" };
            status.Candidates.AddRange(["受付,10", "完了,30"]);
            sale.Fields.Add(status);
            sale.Fields.Add(new DateFieldDesign { Name = "SoldOn", DbColumn = "sold_on" });
            sale.Fields.Add(new TextFieldDesign { Name = "Owner", DbColumn = "owner" });
            ((IEditableModuleDesign)d.Modules).Add(sale);

            var page = new ModuleDesign { Name = "Page" };
            var chart = new ApexAggregateChartFieldDesign { Name = "Chart", CategoryField = "SoldOn", CategoryDateUnit = ChartDateUnit.Month, SeriesGroupField = "Status" };
            chart.SearchCondition.ModuleName = "Sale";
            chart.SearchCondition.Condition = Equal("Owner.Value", "A");
            chart.Series.Series.Add(new AggregateSeries { Function = ChartAggregateFunction.Sum, Name = "Amount", Type = SeriesType.Bar });
            page.Fields.Add(chart);
            var pie = new ApexAggregateRadialChartFieldDesign { Name = "Pie", CategoryField = "Status", SeriesFunction = ChartAggregateFunction.Count };
            pie.SearchCondition.ModuleName = "Sale";
            page.Fields.Add(pie);
            addField?.Invoke(page);
            ((IEditableModuleDesign)d.Modules).Add(page);
            return d;
        }

        static FieldValueMatchCondition Equal(string variable, object value)
            => new() { SearchTargetVariable = variable, Comparison = MatchComparison.Equal, Value = MultiTypeValue.Create(value) };

        static AggregateRow Row(decimal value, params object?[] keys)
            => new() { Keys = keys.Select(k => new AggregateKey { Value = MultiTypeValue.Create(k), DisplayText = k as string == "10" ? "受付" : k as string == "30" ? "完了" : string.Empty }).ToList(), Values = [MultiTypeValue.Create(value)] };

        //月 × 状態 の結果 (1 月: 受付 100 / 完了 200、2 月: 完了 300)。カテゴリだけの集計は月ごとの合計、分けるだけの集計は状態ごとの合計
        static List<AggregateResult> Results(List<AggregateCondition> conditions) => conditions.Select(c => c.Groups.Count switch
        {
            2 => new AggregateResult { Rows = [Row(100, new DateOnly(2026, 1, 1), "10"), Row(200, new DateOnly(2026, 1, 1), "30"), Row(300, new DateOnly(2026, 2, 1), "30")] },
            1 when c.Groups[0] is DateGroup => new AggregateResult { Rows = [Row(300, new DateOnly(2026, 1, 1)), Row(300, new DateOnly(2026, 2, 1))] },
            _ => new AggregateResult { Rows = [Row(100, "10"), Row(500, "30")] },
        }).ToList();

        static async Task<(TestServices Svc, ApexAggregateChartField Field)> CreateAsync(string name, DesignData? design = null)
        {
            var svc = new TestServices(design ?? Design());
            svc.App.AggregateProvider = Results;
            var module = await svc.CreateModuleAsync("Page");
            return (svc, module.GetField<ApexAggregateChartField>(name)!);
        }

        //条件の葉 (変数 = 値) を全部
        static List<string> Leaves(MatchConditionBase? c) => c switch
        {
            null => [],
            MultiMatchCondition m => m.Children.SelectMany(Leaves).ToList(),
            FieldValueMatchCondition v => [$"{v.SearchTargetVariable}={v.Value.GetValue()}"],
            _ => [c.GetType().Name],
        };

        [Test]
        public async Task 設計の集計を1往復で取り月をカテゴリに状態ごとの系列にする()
        {
            var (svc, field) = await CreateAsync("Chart");
            await field.ReloadAsync();
            Assert.That(field.LoadError, Is.Empty);
            Assert.That(svc.App.AggregateRequests, Has.Count.EqualTo(1));
            //カテゴリを選ぶ集計 (月)・本体 (月 × 状態)・系列の並び用 (状態) を 1 回で。総計は取らない
            Assert.That(svc.App.AggregateRequests[0].Select(c => c.Groups.Count), Is.EqualTo(new[] { 1, 2, 1 }));
            Assert.That(svc.App.AggregateRequests[0].All(c => c.UtcOffsetMinutes != null), Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(field.SeriesData.Select(d => d.XValue), Is.EqualTo(new[] { "2026-01", "2026-02" }));
                Assert.That(field.Series.Select(s => field.GetSeriesTitle(s.Name)), Is.EqualTo(new[] { "受付", "完了" }));
                Assert.That(field.SeriesData[0].Data.Values, Is.EqualTo(new decimal?[] { 100, 200 }));
                Assert.That(field.SeriesData[1].Data.Values, Is.EqualTo(new decimal?[] { null, 300 }));
            });
        }

        [Test]
        public async Task 追加の条件は設計の条件とANDで集計し直し別モジュールは拒否する()
        {
            var (svc, field) = await CreateAsync("Chart");
            var searcher = new ModuleSearcher("Sale");
            searcher.AddConditions(Equal("Status.Value", "30"));
            await field.SetAdditionalConditionAsync(searcher);
            foreach (var condition in svc.App.AggregateRequests.Last())
                Assert.That(Leaves(condition.Condition), Is.EquivalentTo(new[] { "Owner.Value=A", "Status.Value=30" }));
            Assert.That(() => field.SetAdditionalConditionAsync(new ModuleSearcher("Page")), Throws.TypeOf<LowCodeException>());
        }

        [Test]
        public async Task 検索フィールドからの条件は入れるだけで続く読み直しの1回だけ集計する()
        {
            var (svc, field) = await CreateAsync("Chart");
            //検索フィールドの検索と同じ呼び方: 条件を渡してから ReloadAsync
            ISearchResultsViewField view = field;
            var condition = new SearchCondition("Sale") { Condition = Equal("Status.Value", "30") };
            await view.SetAdditionalConditionAsync(condition, 0);
            Assert.That(svc.App.AggregateRequests, Is.Empty);
            await view.ReloadAsync();
            Assert.That(svc.App.AggregateRequests, Has.Count.EqualTo(1));
            Assert.That(Leaves(svc.App.AggregateRequests[0][0].Condition), Is.EquivalentTo(new[] { "Owner.Value=A", "Status.Value=30" }));
        }

        [Test]
        public async Task 検索フィールドの表示先なら初期表示の集計は検索フィールドの初回検索の1回だけ()
        {
            //画面に 集計チャート と それを表示先にした検索フィールド を置く
            DesignData WithSearch(bool placeSearch) => Design(page =>
            {
                page.Fields.Add(new SearchFieldDesign { Name = "Search", ResultsViewFieldName = "Chart" });
                page.DetailLayouts[""] = new DetailLayoutDesign();
                page.DetailLayouts[""].DataOnlyFields.Add("Chart");
                if (placeSearch) page.DetailLayouts[""].DataOnlyFields.Add("Search");
            });
            var svc = new TestServices(WithSearch(placeSearch: true));
            svc.App.AggregateProvider = Results;
            var module = await svc.CreateModuleAsync("Page", ModuleLayoutType.Detail);
            Assert.That(svc.App.AggregateRequests, Has.Count.EqualTo(1));
            Assert.That(module.GetField<ApexAggregateChartField>("Chart")!.Series, Is.Not.Empty);

            //検索フィールドを画面に置いていなければ自分で集計する
            svc = new TestServices(WithSearch(placeSearch: false));
            svc.App.AggregateProvider = Results;
            await svc.CreateModuleAsync("Page", ModuleLayoutType.Detail);
            Assert.That(svc.App.AggregateRequests, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task 集計値を読めるようツールチップを出しツールバーは出さない()
        {
            var design = Design(page =>
            {
                page.DetailLayouts[""] = new DetailLayoutDesign();
                page.DetailLayouts[""].DataOnlyFields.Add("Chart");
            });
            var svc = new TestServices(design);
            svc.App.AggregateProvider = Results;
            var module = await svc.CreateModuleAsync("Page", ModuleLayoutType.Detail);
            var field = module.GetField<ApexAggregateChartField>("Chart")!;
            Assert.That(field.Options.Tooltip?.Enabled, Is.True);
            Assert.That(field.Options.Chart.Toolbar?.Show, Is.False);
        }

        static SimpleLambdaExpressionSyntax L(string text) => (SimpleLambdaExpressionSyntax)SyntaxFactory.ParseExpression(text);

        [Test]
        public async Task Showの定義は設計より優先し先頭の軸がカテゴリで見出しは項目の表示名と集計方法()
        {
            var (svc, field) = await CreateAsync("Chart");
            var aggregator = new ModuleAggregator("Sale");
            aggregator.GroupBy(L("m => m.Status"));
            aggregator.Sum(L("m => m.Amount"));
            await field.ShowAsync(aggregator);
            var sent = svc.App.AggregateRequests.Last();
            Assert.Multiple(() =>
            {
                Assert.That(sent[0].Groups.Single().Variable, Is.EqualTo("Status.Value"));
                //設計の条件は使わない (Show の定義の条件だけ)
                Assert.That(Leaves(sent[0].Condition), Is.Empty);
                Assert.That(field.SeriesData.Select(d => d.XValue), Is.EqualTo(new[] { "受付", "完了" }));
                Assert.That(field.GetSeriesTitle(field.Series.Single().Name), Is.EqualTo("金額 " + ChartAggregateFunction.Sum.GetDisplayName()));
            });
        }

        [Test]
        public async Task 円は値を1つだけ描き件数の見出しになる()
        {
            var (svc, field) = await CreateAsync("Pie");
            await field.ReloadAsync();
            Assert.That(svc.App.AggregateRequests.Last()[0].Measures.Single().Function, Is.EqualTo(AggregateFunction.Count));
            Assert.That(field.Series.Single().Type, Is.EqualTo(SeriesType.Pie));
            Assert.That(field.SeriesData.Select(d => d.XValue), Is.EqualTo(new[] { "受付", "完了" }));

            //Show で値を 2 つ渡しても円は先頭だけ
            var aggregator = new ModuleAggregator("Sale");
            aggregator.GroupBy(L("m => m.Status"));
            aggregator.Count();
            aggregator.Sum(L("m => m.Amount"));
            await field.ShowAsync(aggregator, 0);
            Assert.That(svc.App.AggregateRequests.Last()[0].Measures, Has.Count.EqualTo(1));
            Assert.That(svc.App.AggregateRequests.Last()[0].Groups, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task 集計が失敗したらグラフを空にしてLoadErrorに文言を入れ成功したら消す()
        {
            var (svc, field) = await CreateAsync("Chart");
            //一度描いたグラフも、失敗したら前の結果を残さない
            await field.ReloadAsync();
            Assert.That(field.Series, Is.Not.Empty);
            svc.App.AggregateProvider = _ => throw new LowCodeException("集計できません");
            await field.ReloadAsync();
            Assert.Multiple(() =>
            {
                Assert.That(field.LoadError, Is.EqualTo("集計できません"));
                Assert.That(field.Series, Is.Empty);
                Assert.That(field.IsLoading, Is.False);
            });
            svc.App.AggregateProvider = Results;
            await field.ReloadAsync();
            Assert.That(field.LoadError, Is.Empty);
            Assert.That(field.Series, Is.Not.Empty);
        }
    }
}
