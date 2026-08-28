using OpenQA.Selenium;
using Selenium.StandardControls;
using Selenium.StandardControls.PageObjectUtility;

namespace Codeer.LowCode.Bindings.ApexCharts.SeleniumDrivers
{
    /// <summary>棒 / 折れ線 / 面 / 散布図 / ヒートマップ。</summary>
    public class ApexChartFieldDriver : ApexChartFieldDriverBase
    {
        public ApexChartFieldDriver(IWebElement element) : base(element) { }
        public static implicit operator ApexChartFieldDriver(ElementFinder finder) => finder.Find<ApexChartFieldDriver>();
    }
}
