using System.Reflection;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Models
{
    /// <summary>
    /// チャートの系列種別。ApexCharts.SeriesType と同名メンバーを持つ、このライブラリ自身の列挙。
    /// デザインのプロパティにはこちらを使い、メンバーの表示名をリソースでローカライズする。
    /// JSON 上の値(メンバー名)は ApexCharts.SeriesType と同一なので既存デザインはそのまま読める。
    /// ApexCharts へ渡すときは <see cref="SeriesTypeExtensions.ToApexSeriesType"/> で変換する。
    /// </summary>
    public enum SeriesType
    {
        [Designer(DisplayName = "$SeriesType_Area")] Area,
        [Designer(DisplayName = "$SeriesType_Bar")] Bar,
        [Designer(DisplayName = "$SeriesType_Donut")] Donut,
        [Designer(DisplayName = "$SeriesType_Heatmap")] Heatmap,
        [Designer(DisplayName = "$SeriesType_Line")] Line,
        [Designer(DisplayName = "$SeriesType_Pie")] Pie,
        [Designer(DisplayName = "$SeriesType_PolarArea")] PolarArea,
        [Designer(DisplayName = "$SeriesType_Radar")] Radar,
        [Designer(DisplayName = "$SeriesType_RadialBar")] RadialBar,
        [Designer(DisplayName = "$SeriesType_Scatter")] Scatter,
        [Designer(DisplayName = "$SeriesType_Treemap")] Treemap,
        [Designer(DisplayName = "$SeriesType_RangeArea")] RangeArea,
    }

    public static class SeriesTypeExtensions
    {
        public static global::ApexCharts.SeriesType ToApexSeriesType(this SeriesType type) => type switch
        {
            SeriesType.Area => global::ApexCharts.SeriesType.Area,
            SeriesType.Bar => global::ApexCharts.SeriesType.Bar,
            SeriesType.Donut => global::ApexCharts.SeriesType.Donut,
            SeriesType.Heatmap => global::ApexCharts.SeriesType.Heatmap,
            SeriesType.Line => global::ApexCharts.SeriesType.Line,
            SeriesType.Pie => global::ApexCharts.SeriesType.Pie,
            SeriesType.PolarArea => global::ApexCharts.SeriesType.PolarArea,
            SeriesType.Radar => global::ApexCharts.SeriesType.Radar,
            SeriesType.RadialBar => global::ApexCharts.SeriesType.RadialBar,
            SeriesType.Scatter => global::ApexCharts.SeriesType.Scatter,
            SeriesType.Treemap => global::ApexCharts.SeriesType.Treemap,
            SeriesType.RangeArea => global::ApexCharts.SeriesType.RangeArea,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        public static SeriesType ToSeriesType(this global::ApexCharts.SeriesType type) => type switch
        {
            global::ApexCharts.SeriesType.Area => SeriesType.Area,
            global::ApexCharts.SeriesType.Bar => SeriesType.Bar,
            global::ApexCharts.SeriesType.Donut => SeriesType.Donut,
            global::ApexCharts.SeriesType.Heatmap => SeriesType.Heatmap,
            global::ApexCharts.SeriesType.Line => SeriesType.Line,
            global::ApexCharts.SeriesType.Pie => SeriesType.Pie,
            global::ApexCharts.SeriesType.PolarArea => SeriesType.PolarArea,
            global::ApexCharts.SeriesType.Radar => SeriesType.Radar,
            global::ApexCharts.SeriesType.RadialBar => SeriesType.RadialBar,
            global::ApexCharts.SeriesType.Scatter => SeriesType.Scatter,
            global::ApexCharts.SeriesType.Treemap => SeriesType.Treemap,
            global::ApexCharts.SeriesType.RangeArea => SeriesType.RangeArea,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        /// <summary>デザイナ表示用のローカライズ済み名称 ([Designer(DisplayName = "$...")] をリソースで解決)。</summary>
        public static string GetDisplayName(this SeriesType type)
        {
            var key = typeof(SeriesType).GetField(type.ToString())?.GetCustomAttribute<DesignerAttribute>()?.DisplayName;
            if (string.IsNullOrEmpty(key) || !key.StartsWith("$")) return type.ToString();
            return Properties.Resources.ResourceManager.GetString(key[1..]) ?? type.ToString();
        }

        /// <summary>表示名またはメンバー名から SeriesType を得る。</summary>
        public static SeriesType? ParseDisplayName(string? text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            foreach (var value in Enum.GetValues<SeriesType>())
            {
                if (value.GetDisplayName() == text) return value;
            }
            return Enum.TryParse<SeriesType>(text, true, out var parsed) ? parsed : null;
        }
    }
}
