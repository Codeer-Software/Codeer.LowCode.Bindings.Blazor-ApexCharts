# ApexRadialChart (ApexRadialChartFieldDesign)

Donut / Pie / PolarArea のような単一系列の円形チャートに対応するフィールドです。

[← README に戻る](../README.md)

## プロパティ

[共通プロパティ](ApexChart.md#共通プロパティ) に加えて、以下を持ちます。

| プロパティ | 型 | 既定値 | 説明 |
| --- | --- | --- | --- |
| `SeriesType` | `SeriesType` | `Bar` (実用上は `Donut` / `Pie` / `PolarArea` のいずれかを設定) | チャートの種別。`Donut` / `Pie` / `PolarArea` の3つのみが Designer のプルダウンに表示されます (他の `Area` / `Bar` / `Line` / `Scatter` / `Heatmap` / `Radar` / `RadialBar` / `Treemap` / `RangeArea` は `EnumIgnore` で除外)。 |
| `SeriesField` | `string?` | (空) | 値として使用する数値フィールド名。`NumberFieldDesign` のみが候補となります。 |

## データバインディング

- カテゴリ (各扇形のラベル) は `CategoryField` の値、サイズは `SeriesField` の値が使用されます。
- `ApexChartFieldDesign` のように複数系列を持つことはできず、`SeriesField` 1本のみが系列となります。

## 配色

[ApexChart のセクション](ApexChart.md#配色) と同じ。系列ごとの色指定はできないため、デフォルトテーマから順次割り当てられます。

## デザインチェック

- `CategoryField` がモジュールに存在するか
- `SeriesField` がモジュールに存在し、`NumberField` であるか
