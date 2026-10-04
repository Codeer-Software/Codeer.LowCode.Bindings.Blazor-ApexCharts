## Design

**TypeFullName:** `Codeer.LowCode.Bindings.ApexCharts.Designs.ApexAggregateHBarChartFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Bindings.Blazor-ApexCharts`

元モジュールの行をカテゴリの項目でまとめて DB 側で集計し、**横棒**で表示する読み取り専用フィールド。「売上上位 10 社」のようなランキングに向く（`CategoryOrder: "ValueDescending"` + `CategoryLimit: 10`）。系列は棒に固定（種類は選べない）。

共通プロパティ・系列の構造・列挙・ランタイム動作・スクリプトは [ApexAggregateChartFieldDesign](ApexAggregateChartFieldDesign.md) と同じ。

### 固有プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Series` | AggregateChartSeries | 空 | 系列の一覧（`Type` は無視して棒で描く） |
| `SeriesGroupField` | string | `""` | 系列を分ける項目の `Name` |
| `SeriesFractionDigits` | int | `2` | 値の軸（X 軸）ラベルの小数桁 |

### JSON例（担当ごとの金額の合計 上位 5）

```json
{
  "SearchCondition": { "SelectFields": [], "SortConditions": [], "SortFieldVariable": "", "SortDescending": false, "ModuleName": "Sale" },
  "DisplayName": "担当別売上 上位 5",
  "CategoryField": "Rep",
  "CategoryDateUnit": "None",
  "FiscalYearStartMonth": 1,
  "CategoryOrder": "ValueDescending",
  "CategoryLimit": 5,
  "ShowLegend": false,
  "Series": { "Series": [ { "Function": "Sum", "Name": "Amount", "Title": "", "Type": "Bar", "Color": "" } ] },
  "SeriesGroupField": "",
  "SeriesFractionDigits": 0,
  "Name": "TopReps",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexAggregateHBarChartFieldDesign"
}
```
