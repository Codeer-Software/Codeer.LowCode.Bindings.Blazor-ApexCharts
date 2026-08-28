using OpenQA.Selenium;
using Selenium.StandardControls;
using Selenium.StandardControls.PageObjectUtility;

namespace Codeer.LowCode.Bindings.ApexCharts.SeleniumDrivers
{
    /// <summary>円 / ドーナツ / ポーラエリア。</summary>
    public class ApexRadialChartFieldDriver : ApexChartFieldDriverBase
    {
        public ApexRadialChartFieldDriver(IWebElement element) : base(element) { }
        public static implicit operator ApexRadialChartFieldDriver(ElementFinder finder) => finder.Find<ApexRadialChartFieldDriver>();
    }
}
