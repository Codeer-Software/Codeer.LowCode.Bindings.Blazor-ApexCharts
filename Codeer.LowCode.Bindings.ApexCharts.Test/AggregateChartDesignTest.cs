using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.DesignLogic.Check;
using Codeer.LowCode.Blazor.DesignLogic.Refactor;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;

namespace Codeer.LowCode.Bindings.ApexCharts.Test
{
    // 集計チャートの設計: デザインチェック / 改名の追従 / JSON の往復
    public class AggregateChartDesignTest
    {
        static (DesignData Design, ApexAggregateChartFieldDesign Field) Create()
        {
            var d = new DesignData();
            var sale = new ModuleDesign { Name = "Sale", DbTable = "sale" };
            sale.Fields.Add(new NumberFieldDesign { Name = "Amount" });
            sale.Fields.Add(new DateFieldDesign { Name = "SoldOn" });
            sale.Fields.Add(new TextFieldDesign { Name = "Status" });
            ((IEditableModuleDesign)d.Modules).Add(sale);

            var field = new ApexAggregateChartFieldDesign { Name = "Chart", CategoryField = "SoldOn", CategoryDateUnit = ChartDateUnit.Month, SeriesGroupField = "Status" };
            field.SearchCondition.ModuleName = "Sale";
            field.Series.Series.Add(new AggregateSeries { Function = ChartAggregateFunction.Sum, Name = "Amount" });
            var page = new ModuleDesign { Name = "Page" };
            page.Fields.Add(field);
            ((IEditableModuleDesign)d.Modules).Add(page);
            return (d, field);
        }

        static List<string> Codes(DesignData d, FieldDesignBase field)
            => field.CheckDesign(new DesignCheckContext("Page", d, new())).Select(e => e.Code).ToList();

        static string Code(int number) => DesignCheckCode.Create(typeof(ApexAggregateChartFieldDesignBase), number);

        [Test]
        public void 正しい設計は指摘なしで元モジュールが空ならスクリプト用で指摘なし()
        {
            var (d, field) = Create();
            Assert.That(field.CheckDesign(new DesignCheckContext("Page", d, new())), Is.Empty);

            var scriptOnly = new ApexAggregateChartFieldDesign { Name = "Scripted" };
            d.Modules.Find("Page")!.Fields.Add(scriptOnly);
            var infos = scriptOnly.CheckDesign(new DesignCheckContext("Page", d, new()));
            Assert.That(infos, Is.Empty, string.Join(",", infos.Select(e => e.Code + " " + e.Message)));
        }

        [Test]
        public void カテゴリなし_系列なし_無い項目_日付でない項目の単位は指摘される()
        {
            var (d, field) = Create();
            field.CategoryField = string.Empty;
            field.Series.Series.Clear();
            Assert.That(Codes(d, field), Is.SupersetOf(new[] { Code(ApexAggregateChartFieldDesignBase.Codes.NoCategory), Code(ApexAggregateChartFieldDesignBase.Codes.NoSeries) }));

            (d, field) = Create();
            field.CategoryField = "Status";
            field.SeriesGroupField = "Nothing";
            var infos = field.CheckDesign(new DesignCheckContext("Page", d, new()));
            Assert.That(infos.Select(e => e.Code), Does.Contain(Code(ApexAggregateChartFieldDesignBase.Codes.DateUnitRequiresDate)));
            //無い項目は本体の存在確認が出す
            Assert.That(infos.Any(e => e.Message.Contains("Nothing")), Is.True);
        }

        static readonly string NotTableModule = Code(ApexAggregateChartFieldDesignBase.Codes.NotTableModule);

        [Test]
        public void QueryFieldで定義したモジュールは集計できないと指摘される()
        {
            var (d, field) = Create();
            d.Modules.Find("Sale")!.Fields.Add(new QueryFieldDesign { Name = "Query" });
            var info = field.CheckDesign(new DesignCheckContext("Page", d, new())).Single(e => e.Code == NotTableModule);
            Assert.That(((FieldDesignCheckInfo)info).Location.Member, Is.EqualTo(nameof(ApexAggregateChartFieldDesignBase.SearchCondition)));
        }

        [Test]
        public void テーブルの無いモジュールは集計できないと指摘される()
        {
            var (d, field) = Create();
            d.Modules.Find("Sale")!.DbTable = string.Empty;
            Assert.That(Codes(d, field).Count(e => e == NotTableModule), Is.EqualTo(1));
        }

        [Test]
        public void テーブルを持つモジュールと元モジュールが空の設計はテーブルの指摘なし()
        {
            var (d, field) = Create();
            Assert.That(Codes(d, field), Does.Not.Contain(NotTableModule));
            var scriptOnly = new ApexAggregateChartFieldDesign { Name = "Scripted" };
            d.Modules.Find("Page")!.Fields.Add(scriptOnly);
            Assert.That(Codes(d, scriptOnly), Does.Not.Contain(NotTableModule));
        }

        [Test]
        public void 元モジュールの項目の改名にカテゴリと系列と系列を分ける項目が追従する()
        {
            var (d, field) = Create();
            foreach (var (source, destination) in new[] { ("SoldOn", "OrderDate"), ("Amount", "Price"), ("Status", "State") })
            {
                var result = field.ChangeName(new RenameContext(d) { Type = RenameType.Field, ModuleName = "Sale", OwnerModule = "Page", Source = source, Destination = destination });
                Assert.That(result.RenameNeeded, source);
                result.RenameAction();
            }
            Assert.Multiple(() =>
            {
                Assert.That(field.CategoryField, Is.EqualTo("OrderDate"));
                Assert.That(field.Series.Series[0].Name, Is.EqualTo("Price"));
                Assert.That(field.SeriesGroupField, Is.EqualTo("State"));
            });
            //関係のない項目・別モジュールでは変わらない
            Assert.That(field.ChangeName(new RenameContext(d) { Type = RenameType.Field, ModuleName = "Other", OwnerModule = "Page", Source = "Price", Destination = "X" }).RenameNeeded, Is.False);
        }

        [Test]
        public void 元モジュールの改名と円の項目の改名に追従する()
        {
            var (d, field) = Create();
            var result = field.ChangeName(new RenameContext(d) { Type = RenameType.Module, Source = "Sale", Destination = "SalesOrder" });
            Assert.That(result.RenameNeeded);
            result.RenameAction();
            Assert.That(field.SearchCondition.ModuleName, Is.EqualTo("SalesOrder"));

            var radial = new ApexAggregateRadialChartFieldDesign { Name = "Pie", CategoryField = "Status", SeriesFunction = ChartAggregateFunction.Sum, SeriesField = "Amount" };
            radial.SearchCondition.ModuleName = "Sale";
            result = radial.ChangeName(new RenameContext(d) { Type = RenameType.Field, ModuleName = "Sale", OwnerModule = "Page", Source = "Amount", Destination = "Price" });
            Assert.That(result.RenameNeeded);
            result.RenameAction();
            Assert.That(radial.SeriesField, Is.EqualTo("Price"));
        }

        [Test]
        public void JSONを往復しても設定が保たれる()
        {
            var (_, field) = Create();
            field.CategoryOrder = ChartCategoryOrder.ValueAscending;
            field.CategoryLimit = 7;
            field.FiscalYearStartMonth = 4;
            field.Series.Series[0].Title = "売上";
            field.Series.Series[0].Type = SeriesType.Area;
            var json = JsonConverterEx.SerializeObject(field);
            var back = JsonConverterEx.DeserializeObject<FieldDesignBase>(json) as ApexAggregateChartFieldDesign;
            Assert.That(back, Is.Not.Null);
            Assert.That(JsonConverterEx.SerializeObject(back), Is.EqualTo(json));
        }
    }
}
