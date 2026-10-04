## Design

**TypeFullName:** `Codeer.LowCode.Bindings.ApexCharts.Designs.ApexAggregateRadialChartFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Bindings.Blazor-ApexCharts`

元モジュールの行をカテゴリの項目でまとめて DB 側で集計し、**円・ドーナツ・ポーラー**で割合を見せる読み取り専用フィールド。値は 1 つ（集計方法 + 項目）。系列を分ける項目・軸・グリッドは無い。

共通プロパティ（SearchCondition / DisplayName / CategoryField / CategoryDateUnit / FiscalYearStartMonth / CategoryOrder / CategoryLimit / ShowLegend）・ランタイム動作は [ApexAggregateChartFieldDesign](ApexAggregateChartFieldDesign.md) と同じ。

### 固有プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `SeriesType` | SeriesType | `"Pie"` | `Pie` / `Donut` / `PolarArea` |
| `SeriesFunction` | ChartAggregateFunction | `"Count"` | `Sum` / `Count` / `Avg` / `Min` / `Max` |
| `SeriesField` | string | `""` | 集計する数値の項目の `Name`（`Count` のときは不要） |

### JSON例（状態ごとの件数）

```json
{
  "SearchCondition": { "SelectFields": [], "SortConditions": [], "SortFieldVariable": "", "SortDescending": false, "ModuleName": "Sale" },
  "DisplayName": "状態別件数",
  "CategoryField": "Status",
  "CategoryDateUnit": "None",
  "FiscalYearStartMonth": 1,
  "CategoryOrder": "Field",
  "CategoryLimit": 0,
  "ShowLegend": true,
  "SeriesType": "Donut",
  "SeriesFunction": "Count",
  "SeriesField": "",
  "Name": "StatusPie",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexAggregateRadialChartFieldDesign"
}
```

## Script

`Show(agg)` / `Reload()` / `SetAdditionalCondition(searcher)` / `AllowLoad` / `LoadError` は [ApexAggregateChartFieldDesign](ApexAggregateChartFieldDesign.md) と同じ。円は系列を分けないので、`Show` の軸は全部カテゴリになり、値は先頭の 1 つだけを描く。
