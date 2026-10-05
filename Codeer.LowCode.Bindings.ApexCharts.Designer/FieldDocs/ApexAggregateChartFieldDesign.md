## Design

**TypeFullName:** `Codeer.LowCode.Bindings.ApexCharts.Designs.ApexAggregateChartFieldDesign`

**外部ライブラリ:** `Codeer.LowCode.Bindings.Blazor-ApexCharts`（ApexCharts ベースのチャートコンポーネント）

元モジュール（`SearchCondition.ModuleName`）の行を**カテゴリの項目でまとめて DB 側で集計**し、棒・折れ線・面・ヒートマップで表示する読み取り専用フィールド。集計は本体の集計 API で行い、権限は一覧と同じ（読めない行は数えない・読めない項目は使えない）。行を全部ブラウザに読まないので、行数が多くても重くならない。

- 月別の売上推移・状態別の件数・担当ごとの合計など「行をまとめて数える」グラフはこのフィールドを使う（集計用の QueryField モジュールは不要）
- 行をそのまま点にするグラフ（散布図・1 行 = 1 点の推移）は [ApexChartFieldDesign](ApexChartFieldDesign.md)
- 横棒のランキングは [ApexAggregateHBarChartFieldDesign](ApexAggregateHBarChartFieldDesign.md)、円・ドーナツは [ApexAggregateRadialChartFieldDesign](ApexAggregateRadialChartFieldDesign.md)

### プロパティ

> 共通プロパティ（Name）は [_FieldCommon.md](_FieldCommon.md) を参照。

#### 集計チャート 3 種の共通プロパティ（ApexAggregateChartFieldDesignBase）

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `SearchCondition` | SearchCondition | 空 | 元モジュールと条件。**並び・件数上限は無視する**（条件だけ使う）。`ModuleName` を空にするとスクリプトの `Show` 専用 |
| `DisplayName` | string | `""` | グラフのタイトル |
| `CategoryField` | string | `""` | カテゴリ（X 軸）にする元モジュールの項目の `Name`。文字・数値・日付・日時・選択・リンク・真偽。選択とリンクは表示名で出る |
| `CategoryDateUnit` | ChartDateUnit | `"None"` | 日付・日時のカテゴリをまとめる単位 |
| `FiscalYearStartMonth` | int | `1` | `Year` / `Quarter` のときの年度の開始月 (1〜12) |
| `CategoryOrder` | ChartCategoryOrder | `"Field"` | カテゴリの並び。値の順は先頭の系列の値で比べる |
| `CategoryLimit` | int | `0` | カテゴリの上限 (0 = なし)。`CategoryOrder` と組み合わせて上位 N 件 |
| `ShowLegend` | bool | `true` | 凡例を表示 |
| `CanCustomize` | bool | `false` | 閲覧者が画面で集計を自分用に変えられる（右上に「集計のカスタマイズ」ボタン。下記） |

#### ApexAggregateChartFieldDesign 固有プロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Series` | AggregateChartSeries | 空 | 系列の一覧（下記） |
| `SeriesGroupField` | string | `""` | 系列を分ける項目の `Name`。指定するとその値ごとに系列を作る（例: 状態ごとの系列） |
| `SeriesFractionDigits` | int | `2` | 値の軸ラベルの小数桁 |
| `ShowXAxisGrid` | bool | `false` | X 軸のグリッド線 |
| `ShowYAxisGrid` | bool | `true` | Y 軸のグリッド線 |

#### Series（AggregateChartSeries）の構造

```json
"Series": { "Series": [ { "Function": "Sum", "Name": "Amount", "Title": "", "Type": "Bar", "Color": "" } ] }
```

| プロパティ | 型 | 説明 |
|---|---|---|
| `Function` | ChartAggregateFunction | `Sum` / `Count` / `Avg` / `Min` / `Max` |
| `Name` | string | 集計する**数値の項目**の `Name`。`Count` のときは空 |
| `Title` | string | 凡例の見出し。空なら「項目の表示名 集計方法」（件数は「件数」） |
| `Type` | SeriesType | `Bar` / `Line` / `Area` / `Heatmap`（ヒートマップは他と混在不可） |
| `Color` | string | `#RRGGBB`。空なら既定の色を順に |

`SeriesGroupField` を指定したときは、系列 = 値 × 分けた値。色は既定の色を順に使い、種類は値の `Type`。見出しは分けた値（値が複数なら「分けた値 / 値の見出し」）。

### 列挙型

| 列挙 | 値 |
|---|---|
| ChartDateUnit | `None` (まとめない) / `Year` / `Quarter` / `Month` / `Week` (月曜始まり) / `Day` / `Hour` |
| ChartCategoryOrder | `Field` (項目の順: 選択は候補の順・リンクは表示名の順・その他は値の順・空値は最後) / `ValueDescending` / `ValueAscending` |
| ChartAggregateFunction | `Sum` / `Count` / `Avg` / `Min` / `Max` |

カテゴリの見出し: 年 `2026`・四半期 `2026 Q2`（年度のときは年度の開始年）・月 `2026-04`・週と日 `2026-04-06`・時 `2026-04-06 09:00`。空値は「(空白)」。

### JSON例（月別の金額の合計を状態ごとに積む）

```json
{
  "SearchCondition": { "SelectFields": [], "SortConditions": [], "SortFieldVariable": "", "SortDescending": false, "ModuleName": "Sale" },
  "DisplayName": "月別売上",
  "CategoryField": "SoldOn",
  "CategoryDateUnit": "Month",
  "FiscalYearStartMonth": 1,
  "CategoryOrder": "Field",
  "CategoryLimit": 0,
  "ShowLegend": true,
  "Series": { "Series": [ { "Function": "Sum", "Name": "Amount", "Title": "", "Type": "Bar", "Color": "" } ] },
  "SeriesGroupField": "Status",
  "SeriesFractionDigits": 0,
  "ShowXAxisGrid": false,
  "ShowYAxisGrid": true,
  "Name": "MonthlySales",
  "TypeFullName": "Codeer.LowCode.Bindings.ApexCharts.Designs.ApexAggregateChartFieldDesign"
}
```

### ランタイム動作

- 画面に置いたときに 1 往復で集計して描く。UTC で保存した日時はブラウザの時差で区切る
- 検索フィールドの結果の表示先にできる（検索条件が集計の条件に AND で入る）。一覧ページの表示（ListPageDesign）にはできない
- `SearchCondition` の条件が画面の項目を参照していれば、その項目が変わると集計し直す
- 集計に失敗したらグラフの位置にエラー文を出す（集計 API に未対応のホストも同じ）
- ツールチップ（集計値）は既定で出す。ツールバーは出さない
- 集計できるのはテーブルを持つモジュールだけ（QueryField で定義したモジュールは使えない）
- SQLite では日付の列を DATE / DATETIME で宣言する（TEXT で宣言した列は `CategoryDateUnit` でまとめられない）
- `CanCustomize: true` なら右上に「集計のカスタマイズ」ボタンを出し、閲覧者が カテゴリの項目（リンク先 1 段も可）・まとめる単位・年度の開始月・系列を分ける項目・系列（集計方法・項目・見出し・種類）・並び・上限 を自分用に変えられる。保存先はブラウザの localStorage（キー「モジュール名.フィールド名」）で、その人の画面だけに効く。元モジュール・検索条件・表示の設定（タイトル・凡例・軸）・系列の色は変えられない。横棒は棒のまま、円は値 1 つ・種類は Pie / Donut / PolarArea。保存後に設計から項目が消えたら保存内容は使わず設計どおり。スクリプトの `Show` で定義を渡したグラフではカスタマイズしない。全員に同じ集計を見せたいなら設計で設定する（カスタマイズは利用者ごとの見方の変更用）

### デザインチェック

`ApexAggregateChartFieldDesignBase:1` カテゴリ未設定 / `:2` 日付でない項目にまとめる単位 / `:3` 数値でない項目の集計 / `:4` ヒートマップの混在 / `:5` 系列なし / `:6` 元モジュールがテーブルを持たない（QueryField で定義したモジュール・テーブルの無いモジュール）。元モジュールが空ならスクリプト専用なので指摘しない。

## Script

### メソッド

| メソッド | 説明 |
|---|---|
| `Show(ModuleAggregator agg)` | スクリプトで組んだ集計を表示する。軸は全部カテゴリ（複数なら「A / B」）。設計の定義より優先 |
| `Show(ModuleAggregator agg, int categoryCount)` | 軸の先頭 `categoryCount` 個をカテゴリに、残りを系列の分割にする |
| `Reload()` | 集計し直す |
| `ShowCustomDialog()` | 集計のカスタマイズのダイアログを開く（`CanCustomize` がオンで `Show` を使っていないときだけ） |
| `SetAdditionalCondition(ModuleSearcher searcher)` | 追加の条件 (AND)。その場で集計し直す。元モジュールと違うモジュールは例外 |
| `AddAnnotation(name, ChartAnnotation)` / `RemoveAnnotation(name)` / `ClearAnnotation()` | 基準線（[ApexChartFieldDesign](ApexChartFieldDesign.md) と同じ） |

### プロパティ

| プロパティ | 説明 |
|---|---|
| `AllowLoad` | false なら集計しない |
| `Options` | ApexCharts のオプション（初期化の後に変更する） |
| `LoadError` | 集計に失敗したときの文言（成功なら空） |

```csharp
// 担当 × 状態 の件数を、担当をカテゴリ・状態を系列にして表示
var agg = new ModuleAggregator<Sale>();
agg.GroupBy(m => m.Rep);
agg.GroupBy(m => m.Status);
agg.Count();
SalesChart.Show(agg, 1);
```
