using OpenQA.Selenium;
using Selenium.StandardControls;
using Selenium.StandardControls.PageObjectUtility;

namespace Codeer.LowCode.Bindings.ApexCharts.SeleniumDrivers
{
    /// <summary>横棒チャート。</summary>
    public class ApexHBarChartFieldDriver : ApexChartFieldDriverBase
    {
        public ApexHBarChartFieldDriver(IWebElement element) : base(element) { }
        public static implicit operator ApexHBarChartFieldDriver(ElementFinder finder) => finder.Find<ApexHBarChartFieldDriver>();
    }
}
