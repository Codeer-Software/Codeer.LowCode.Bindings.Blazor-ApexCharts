# ApexChart (ApexChartFieldDesign)

Bar / Line / Area / Heatmap / Scatter などの一般的なチャートに対応する標準フィールドです。
1つのチャートに複数の系列を持たせ、系列ごとにタイプを切り替えてミックスチャートとして表示することもできます。

[← README に戻る](../README.md)

## 共通プロパティ

`ApexChartFieldDesign` / `ApexHBarChartFieldDesign` / `ApexRadialChartFieldDesign` の3種すべてに共通するプロパティです。
基底クラス `ApexChartFieldDesignBase` で定義されています。

Designer 上の表示名 (日本語) を「表示名」列に示します。

| プロパティ | 表示名 | 型 | 既定値 | 説明 |
| --- | --- | --- | --- | --- |
| `SearchCondition` | 検索条件 | `SearchCondition` | (空) | チャートのデータ取得条件。`ModuleName` には系列となる数値フィールドを持つモジュールを指定します。未設定時、デザインモードでは "ModuleName is not set" バナーが表示されます。 |
| `DisplayName` | 表示名 | `string` | (空) | チャートのタイトルとして描画される表示名。 |
| `CategoryField` | カテゴリフィールド | `string?` | (空) | X軸 (カテゴリ軸) に使用するフィールド名。`TextField` / `NumberField` / `DateField` / `DateTimeField` に対応します。未指定時はレコードのインデックス番号が使用されます。 |
| `CategoryFormat` | カテゴリ書式 | `string?` | (空) | `CategoryField` の値を文字列化する際の書式。値の `ToString(string)` に渡されます (例: `DateTime` に `yyyy/MM` など)。 |
| `SeriesFractionDigits` | 系列の小数桁数 | `int` | `2` | 値軸ラベルの小数桁数 (`Number(value).toFixed(N)`)。横棒チャートでは X 軸、それ以外は Y 軸に作用します。 |
| `ShowLegend` | 凡例を表示 | `bool` | `true` | 凡例 (legend) の表示。`false` の場合は非表示。表示位置は固定で `Bottom`。 |

Designer の共通プロパティのうち `IgnoreModification` / `OnValidateInput` は、チャートでは意味を持たないため表示されません。

## ApexChartFieldDesign 固有のプロパティ

Designer 上のフィールド名は「チャート」です。

| プロパティ | 表示名 | 型 | 既定値 | 説明 |
| --- | --- | --- | --- | --- |
| `Series` | 系列 | `ChartSeries` | (空リスト) | 系列の一覧。系列ごとに `Name` (モジュールの数値フィールド名) / `Color` (HEX) / `Type` (`SeriesType`) を持ちます。Designer 上では専用のダイアログで編集できます。ダイアログで追加した系列の `Type` の既定値は `Line` です。 |
| `FullWidthBar` | 棒を全幅にする (カテゴリ「ApexCharts - 棒」) | `bool` | `false` | Bar 系列のカラム幅を `100%` にします。ヒストグラム表示などに使用します。 |
| `ShowXAxisGrid` | X軸グリッドを表示 (カテゴリ「ApexCharts - グリッド」) | `bool` | `false` | X軸グリッド線の表示。 |
| `ShowYAxisGrid` | Y軸グリッドを表示 (カテゴリ「ApexCharts - グリッド」) | `bool` | `true` | Y軸グリッド線の表示。 |

デザインファイル (JSON) にはフィールド単位の `SeriesType` も保存されますが、Designer には表示されず、描画する種類には使われません。系列の種類は `Series` の各系列の `Type` で決まります。

### 対応する SeriesType

| SeriesType | 集計方式 | 備考 |
| --- | --- | --- |
| `Bar` | `YAggregate` (合計) | 標準の縦棒。`FullWidthBar=true` で 100% 幅。 |
| `Line` | `YAggregate` | |
| `Area` | `YAggregate` | |
| `Heatmap` | `YAggregate` | 他のタイプと混在不可。先頭の系列が `Heatmap` なら `Heatmap` の系列だけを、先頭が `Heatmap` 以外なら `Heatmap` 以外の系列だけを描画します。デザインチェックで警告。 |
| `Scatter` | `YValue` (個別) | マーカーサイズは `5,5,5,5` 固定。 |

> `Treemap` / `RangeArea` / `Radar` / `RadialBar` は enum 上は選択可能ですが現バージョンでは描画ロジックがありません。
> Donut / Pie / PolarArea は [ApexRadialChartFieldDesign](ApexRadialChart.md) を使用してください。

`SeriesType` はこのライブラリの列挙型 `Codeer.LowCode.Bindings.ApexCharts.Models.SeriesType` です (Blazor-ApexCharts の `ApexCharts.SeriesType` ではありません)。C# から Blazor-ApexCharts に渡すときは `ToApexSeriesType()` で変換します。

## 配色

各系列の `Color` が空の場合、以下のデフォルトテーマから順番に割り当てられます (6番目以降は循環)。

```
#008FFB, #00E396, #FEB019, #FF4560, #775DD0
```

文字色 (`Chart.ForeColor`) と背景色 (`Chart.Background`) は、フィールドの `Color` / `BackgroundColor` が設定されていればそれを、無ければレイアウトで設定された色を使います。
色はチャートの初期化時に反映されるため、表示後にスクリプトで色を変えても描画中のチャートには反映されません。

## 表示上の既定

- チャートの高さは配置したセルの高さの 100% です。行に十分な高さを設定してください。
- ツールバー (ズーム・ダウンロード等) とツールチップは無効です。必要な場合はスクリプトから `Options` で変更します ([docs/Scripting.md](Scripting.md#スクリプトから扱えるプロパティ))。

## アノテーション (基準線)

スクリプトから X 軸 / Y 軸の基準線を動的に追加できます。詳細は [docs/Scripting.md](Scripting.md#アノテーション) を参照。

## ヘルパー

`ChartSeries` プロパティの Designer 編集 UI (`ChartSeriesPropertyControl`) は次の機能を提供します。

- 系列の追加 / 削除
- 系列名 (`Name`) を `SearchCondition.ModuleName` の `NumberField` 候補からプルダウン選択
- 系列タイプ (`Type`) のプルダウン選択 (`Area` / `Bar` / `Heatmap` / `Line` / `Scatter`。日本語環境では「面 / 棒 / ヒートマップ / 折れ線 / 散布図」と表示)
- カラーピッカーによる `Color` 指定 (`#RRGGBB`)

## デザインチェック

- `CategoryField` がモジュールに存在するか
- 各系列の `Name` がモジュールの数値フィールドとして存在するか
- `Heatmap` 系列と非 `Heatmap` 系列の混在 (`"Heatmap series and non-heatmap series cannot be mixed."`)。指摘コードは `ApexChartFieldDesign:1` で、意図して混在させる場合は、Designer の出力ペインで指摘を右クリックし「この指摘を抑止」で抑止できます (Codeer.LowCode.Blazor 1.3.27 以降)
