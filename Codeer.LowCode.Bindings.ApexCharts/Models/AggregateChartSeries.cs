using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Models
{
    /// <summary>集計チャートの集計方法。件数は項目なし、それ以外は数値の項目。</summary>
    public enum ChartAggregateFunction
    {
        [Designer(DisplayName = "$ChartAggregateFunction_Sum")] Sum,
        [Designer(DisplayName = "$ChartAggregateFunction_Count")] Count,
        [Designer(DisplayName = "$ChartAggregateFunction_Avg")] Avg,
        [Designer(DisplayName = "$ChartAggregateFunction_Min")] Min,
        [Designer(DisplayName = "$ChartAggregateFunction_Max")] Max,
    }

    /// <summary>日付・日時のカテゴリをまとめる単位。None はまとめない (値そのまま)。</summary>
    public enum ChartDateUnit
    {
        [Designer(DisplayName = "$ChartDateUnit_None")] None,
        [Designer(DisplayName = "$ChartDateUnit_Year")] Year,
        [Designer(DisplayName = "$ChartDateUnit_Quarter")] Quarter,
        [Designer(DisplayName = "$ChartDateUnit_Month")] Month,
        [Designer(DisplayName = "$ChartDateUnit_Week")] Week,
        [Designer(DisplayName = "$ChartDateUnit_Day")] Day,
        [Designer(DisplayName = "$ChartDateUnit_Hour")] Hour,
    }

    /// <summary>カテゴリの並び。値の順は先頭の系列の値で比べる。</summary>
    public enum ChartCategoryOrder
    {
        [Designer(DisplayName = "$ChartCategoryOrder_Field")] Field,
        [Designer(DisplayName = "$ChartCategoryOrder_ValueDescending")] ValueDescending,
        [Designer(DisplayName = "$ChartCategoryOrder_ValueAscending")] ValueAscending,
    }

    /// <summary>集計チャートの系列 1 つ (集計方法 + 項目)。Title が空なら「項目 集計方法」を見出しにする。</summary>
    public class AggregateSeries
    {
        public ChartAggregateFunction Function { get; set; } = ChartAggregateFunction.Sum;
        /// <summary>集計する数値の項目 (件数のときは空)。</summary>
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public SeriesType Type { get; set; } = SeriesType.Bar;
        public string Color { get; set; } = string.Empty;
    }

    public static class ChartAggregateFunctionExtensions
    {
        /// <summary>表示用のローカライズ済み名称 ([Designer(DisplayName = "$...")] をリソースで解決)。</summary>
        public static string GetDisplayName(this ChartAggregateFunction function)
        {
            var key = typeof(ChartAggregateFunction).GetField(function.ToString())?.GetCustomAttributes(typeof(DesignerAttribute), false).OfType<DesignerAttribute>().FirstOrDefault()?.DisplayName;
            if (string.IsNullOrEmpty(key) || !key.StartsWith("$")) return function.ToString();
            return Properties.Resources.ResourceManager.GetString(key[1..]) ?? function.ToString();
        }

        /// <summary>表示名またはメンバー名から集計方法を得る。</summary>
        public static ChartAggregateFunction? ParseDisplayName(string? text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            foreach (var value in Enum.GetValues<ChartAggregateFunction>())
            {
                if (value.GetDisplayName() == text) return value;
            }
            return Enum.TryParse<ChartAggregateFunction>(text, true, out var parsed) ? parsed : null;
        }
    }

    public class AggregateChartSeries : ICurrentSettingsText
    {
        public List<AggregateSeries> Series { get; set; } = [];

        public string GetCurrentSettings()
            => string.Join(", ", Series.Select(s => s.Function == ChartAggregateFunction.Count ? s.Function.ToString() : $"{s.Name} {s.Function}"));
    }
}
