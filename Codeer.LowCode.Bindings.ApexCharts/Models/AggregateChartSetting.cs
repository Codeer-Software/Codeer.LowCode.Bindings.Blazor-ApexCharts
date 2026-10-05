namespace Codeer.LowCode.Bindings.ApexCharts.Models
{
    /// <summary>
    /// 集計チャートの集計の設定 (設計の該当プロパティと同じ内容)。設計から作るか、利用者のカスタマイズとしてブラウザ (localStorage) に JSON で保存する。
    /// キーは「モジュール名.フィールド名」(クロス集計・ListField のカラムカスタマイズと同じ)。
    /// </summary>
    public class AggregateChartSetting
    {
        public string CategoryField { get; set; } = string.Empty;
        public ChartDateUnit CategoryDateUnit { get; set; } = ChartDateUnit.None;
        public int FiscalYearStartMonth { get; set; } = 1;
        /// <summary>系列を分ける項目 (円は使わない)。</summary>
        public string SeriesGroupField { get; set; } = string.Empty;
        /// <summary>系列 (円は先頭の 1 つだけ使う)。</summary>
        public List<AggregateSeries> Series { get; set; } = [];
        public ChartCategoryOrder CategoryOrder { get; set; } = ChartCategoryOrder.Field;
        public int CategoryLimit { get; set; }
    }
}
