# ApexHBarChart (ApexHBarChartFieldDesign)

横棒 (Horizontal Bar) チャートに特化したフィールドです。
内部的には ApexCharts の `PlotOptions.Bar.Horizontal = true` を適用します。

[← README に戻る](../README.md)

## プロパティ

Designer 上のフィールド名は「横棒チャート」です。[共通プロパティ](ApexChart.md#共通プロパティ) に加えて、以下を持ちます。

| プロパティ | 表示名 | 型 | 既定値 | 説明 |
| --- | --- | --- | --- | --- |
| `Series` | 系列 | `ChartSeries` | (空リスト) | 系列の一覧。系列タイプは内部的に `Bar` に固定されます (Designer 上の Type 編集は無効化)。`Name` / `Color` のみ変更可能。 |

デザインファイル (JSON) の `SeriesType` や各系列の `Type` に何を書いても、常に `Bar` として描画されます。

横棒チャートでは:

- X 軸が値、Y 軸がカテゴリとなります。
- `SeriesFractionDigits` は X 軸ラベルの小数桁数として作用します。
- `CategoryField` の値が Y 軸のラベルになります (デザインモードでは `0..4` のインデックス文字列でプレビュー)。

## 配色

[ApexChart のセクション](ApexChart.md#配色) と同じ。

## スクリプト / アノテーション

`Reload()` / `SetAdditionalCondition()` などのスクリプト API は ApexChart と共通です。[docs/Scripting.md](Scripting.md) を参照。
アノテーションは [docs/Scripting.md](Scripting.md#アノテーション) を参照。横棒の場合 `AnnotationAxis.X` は値軸 (横方向)、`AnnotationAxis.Y` はカテゴリ軸 (縦方向) に作用します。

## デザインチェック

- `CategoryField` がモジュールに存在するか
- 各系列の `Name` がモジュールの数値フィールドとして存在するか
