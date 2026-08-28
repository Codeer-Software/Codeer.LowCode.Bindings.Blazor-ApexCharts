using OpenQA.Selenium;
using Selenium.StandardControls;
using Selenium.StandardControls.PageObjectUtility;

namespace Codeer.LowCode.Bindings.ApexCharts.SeleniumDrivers
{
    /// <summary>凡例の1項目。</summary>
    public class ApexLegendItemDriver : ComponentBase
    {
        public string Text => (ByCssSelector(".apexcharts-legend-text").Wait().Find().GetAttribute("textContent") ?? string.Empty).Trim();
        /// <summary>クリックで系列の表示/非表示を切り替える。</summary>
        public void Click() => Element.Click();
        /// <summary>系列名 (seriesname 属性)。</summary>
        public string SeriesName => Element.GetAttribute("seriesname") ?? string.Empty;
        /// <summary>凡例クリックで系列を折り畳んだ状態 (data:collapsed)。折り畳みが無効な構成では常に false。</summary>
        public bool IsCollapsed => Element.GetAttribute("data:collapsed") == "true";
        public ApexLegendItemDriver(IWebElement element) : base(element) { }
        public static implicit operator ApexLegendItemDriver(ElementFinder finder) => finder.Find<ApexLegendItemDriver>();
    }

    /// <summary>描画済みの系列 (svg g.apexcharts-series)。</summary>
    public class ApexSeriesDriver
    {
        public IWebElement Element { get; }
        public ApexSeriesDriver(IWebElement element) => Element = element;
        /// <summary>系列名 (seriesName 属性。ApexCharts は空白を "x" にエスケープする)。</summary>
        public string Name => Element.GetAttribute("seriesName") ?? string.Empty;
        /// <summary>データ点 (棒 / マーカー / スライス) の数。</summary>
        public int PointCount => Element.FindElements(By.CssSelector("[j]")).Count;
        public IReadOnlyList<IWebElement> Points => Element.FindElements(By.CssSelector("[j]")).ToList();
        /// <summary>データ点の値 (val 属性)。</summary>
        public IReadOnlyList<string> Values => Points.Select(p => p.GetAttribute("val") ?? string.Empty).ToList();
    }

    /// <summary>ApexChart 系フィールド共通。チャート本体は ApexCharts.js が描く svg。</summary>
    public abstract class ApexChartFieldDriverBase : ComponentBase
    {
        public IWebElement Canvas => ByCssSelector(".apexcharts-canvas").Wait().Find();
        public bool IsRendered => Element.FindElements(By.CssSelector(".apexcharts-canvas svg")).Count > 0;
        /// <summary>DisplayName (chart title)。未設定なら空文字。</summary>
        public string Title
        {
            get
            {
                var e = Element.FindElements(By.CssSelector(".apexcharts-title-text"));
                return e.Count == 0 ? string.Empty : (e[0].GetAttribute("textContent") ?? string.Empty).Trim();
            }
        }
        public bool HasLegend => Element.FindElements(By.CssSelector(".apexcharts-legend-series")).Count > 0;
        public IReadOnlyList<ApexLegendItemDriver> Legend => Element.FindElements(By.CssSelector(".apexcharts-legend-series")).Select(e => new ApexLegendItemDriver(e)).ToList();
        public IReadOnlyList<string> LegendTexts => Legend.Select(l => l.Text).ToList();
        public IReadOnlyList<ApexSeriesDriver> Series => Element.FindElements(By.CssSelector(".apexcharts-series")).Select(e => new ApexSeriesDriver(e)).ToList();
        /// <summary>全系列のデータ点の合計 (円系は 1 スライス = 1 系列なので Series.Count と同じ)。</summary>
        public int TotalPointCount => Series.Sum(s => s.PointCount);
        /// <summary>X 軸 (カテゴリ) ラベル。</summary>
        public IReadOnlyList<string> XAxisLabels => Element.FindElements(By.CssSelector(".apexcharts-xaxis-texts-g text")).Select(SvgText).ToList();
        public IReadOnlyList<string> YAxisLabels => Element.FindElements(By.CssSelector(".apexcharts-yaxis-texts-g text")).Select(SvgText).ToList();
        /// <summary>X 軸/Y 軸の注釈ラベル文言。</summary>
        public IReadOnlyList<string> AnnotationLabels => Element.FindElements(By.CssSelector(".apexcharts-xaxis-annotation-label, .apexcharts-yaxis-annotation-label")).Select(SvgText).ToList();

        /// <summary>ApexCharts の svg text は &lt;tspan&gt; と &lt;title&gt; に同じ文言を持つため textContent が二重になる。tspan だけ読む。</summary>
        static string SvgText(IWebElement text)
        {
            var spans = text.FindElements(By.TagName("tspan"));
            var s = spans.Count == 0 ? text.GetAttribute("textContent") : string.Concat(spans.Select(t => t.GetAttribute("textContent")));
            return (s ?? string.Empty).Trim();
        }
        protected ApexChartFieldDriverBase(IWebElement element) : base(element) { }
    }
}
