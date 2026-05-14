# ApexChart (ApexChartFieldDesign)

Bar / Line / Area / Heatmap / Scatter などの一般的なチャートに対応する標準フィールドです。
1つのチャートに複数の系列を持たせ、系列ごとにタイプを切り替えてミックスチャートとして表示することもできます。

[← README に戻る](../README.md)

## 共通プロパティ

`ApexChartFieldDesign` / `ApexHBarChartFieldDesign` / `ApexRadialChartFieldDesign` の3種すべてに共通するプロパティです。
基底クラス `ApexChartFieldDesignBase` で定義されています。

| プロパティ | 型 | 既定値 | 説明 |
| --- | --- | --- | --- |
| `SearchCondition` | `SearchCondition` | (空) | チャートのデータ取得条件。`ModuleName` には系列となる数値フィールドを持つモジュールを指定します。未設定時、デザインモードでは "ModuleName is not set" バナーが表示されます。 |
| `DisplayName` | `string` | (空) | チャートのタイトルとして描画される表示名。 |
| `CategoryField` | `string?` | (空) | X軸 (カテゴリ軸) に使用するフィールド名。`TextField` / `NumberField` / `DateField` / `DateTimeField` に対応します。未指定時はレコードのインデックス番号が使用されます。 |
| `CategoryFormat` | `string?` | (空) | `CategoryField` の値を文字列化する際の書式。値の `ToString(string)` に渡されます (例: `DateTime` に `yyyy/MM` など)。 |
| `SeriesFractionDigits` | `int` | `2` | Y軸ラベルの小数桁数 (`Number(value).toFixed(N)`)。`Heatmap` ではフォーマッタは適用されません。 |
| `ShowLegend` | `bool` | `true` | 凡例 (legend) の表示。`false` の場合は非表示。表示位置は固定で `Bottom`。 |

## ApexChartFieldDesign 固有のプロパティ

| プロパティ | 型 | 既定値 | 説明 |
| --- | --- | --- | --- |
| `Series` | `ChartSeries` | (空リスト) | 系列の一覧。系列ごとに `Name` (モジュールの数値フィールド名) / `Color` (HEX) / `Type` (`SeriesType`) を持ちます。Designer 上では専用のダイアログで編集できます。 |
| `FullWidthBar` | `bool` | `false` | Bar 系列のカラム幅を `100%` にします。ヒストグラム表示などに使用します。 |
| `ShowXAxisGrid` | `bool` | `false` | X軸グリッド線の表示。 |
| `ShowYAxisGrid` | `bool` | `true` | Y軸グリッド線の表示。 |

### 対応する SeriesType

| SeriesType | 集計方式 | 備考 |
| --- | --- | --- |
| `Bar` | `YAggregate` (合計) | 標準の縦棒。`FullWidthBar=true` で 100% 幅。 |
| `Line` | `YAggregate` | |
| `Area` | `YAggregate` | |
| `Heatmap` | `YAggregate` | チャート内で混在不可。`Heatmap` を1つでも含めると他タイプは描画されません。デザインチェックで警告。 |
| `Scatter` | `YValue` (個別) | マーカーサイズは `5,5,5,5` 固定。 |

> `Treemap` / `RangeArea` / `Radar` / `RadialBar` は enum 上は選択可能ですが現バージョンでは描画ロジックがありません。
> Donut / Pie / PolarArea は [ApexRadialChartFieldDesign](ApexRadialChart.md) を使用してください。

## 配色

各系列の `Color` が空の場合、以下のデフォルトテーマから順番に割り当てられます (6番目以降は循環)。

```
#008FFB, #00E396, #FEB019, #FF4560, #775DD0
```

文字色 (`Chart.ForeColor`) と背景色 (`Chart.Background`) は、レイアウトで設定されたフィールドの色を継承します。

## アノテーション (基準線)

スクリプトから X 軸 / Y 軸の基準線を動的に追加できます。詳細は [docs/Scripting.md](Scripting.md#アノテーション) を参照。

## ヘルパー

`ChartSeries` プロパティの Designer 編集 UI (`ChartSeriesPropertyControl`) は次の機能を提供します。

- 系列の追加 / 削除
- 系列名 (`Name`) を `SearchCondition.ModuleName` の `NumberField` 候補からプルダウン選択
- 系列タイプ (`Type`) のプルダウン選択 (`Area` / `Bar` / `Heatmap` / `Line` / `Scatter`)
- カラーピッカーによる `Color` 指定 (`#RRGGBB`)

## デザインチェック

- `CategoryField` がモジュールに存在するか
- 各系列の `Name` がモジュールの数値フィールドとして存在するか
- `Heatmap` 系列と非 `Heatmap` 系列の混在 (`"Heatmap series and non-heatmap series cannot be mixed."`)
