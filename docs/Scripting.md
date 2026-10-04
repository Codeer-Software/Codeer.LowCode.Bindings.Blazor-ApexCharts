# スクリプト API

LowCode のスクリプトから `ApexChartField` (3種のフィールド共通の実装クラス) を操作するための API 一覧です。

[← README に戻る](../README.md)

## 目次

- [リロード / 追加検索条件](#リロード--追加検索条件)
- [アノテーション](#アノテーション)
- [スクリプトから扱えるプロパティ](#スクリプトから扱えるプロパティ)
- [集計チャート](#集計チャート)

## リロード / 追加検索条件

### `Reload()`

`SearchCondition` + 追加検索条件 (後述) を使って再取得し、チャートを再描画します。

```csharp
ApexChart1.Reload();
```

`AllowLoad = false` のときは何もしません。

### `SetAdditionalCondition(searcher)`

別画面の `ModuleSearcher` から取得した検索条件を、現在のチャートの `SearchCondition` にマージします。
`ModuleName` が一致しない場合は例外がスローされます (`"{ModuleName} Invalid Module"`)。

```csharp
ApexChart1.SetAdditionalCondition(Searcher1);
ApexChart1.Reload();
```

`SetAdditionalCondition` は条件を設定するだけで、再取得はしません。続けて `Reload()` を呼んでください。
設定した追加検索条件は保持され、以降の `Reload()` や自動リロードにも使われます。別の条件を設定し直すと置き換わります。

外部フィールド (`OnExternalFieldChanged`) と連動して、参照中のフィールドが変わったときに自動でリロードもされます。

### `SetSeriesData(modules)`

検索せずに、渡したモジュールの一覧をデータとして描画します。`CategoryField` の値がカテゴリ、各モジュールの数値フィールドが系列の値になります。

## アノテーション

X軸 / Y軸の基準線をスクリプトから追加・削除できます。
内部的に ApexCharts の `Annotations.Xaxis` / `Annotations.Yaxis` に変換されます。

### `ChartAnnotation` クラス

| プロパティ | 型 | 既定値 | 説明 |
| --- | --- | --- | --- |
| `Axis` | `AnnotationAxis` (`X` / `Y`) | `X` | 配置軸。`AnnotationAxis.X` で縦線、`AnnotationAxis.Y` で横線。 |
| `Value` | `object` | `0` | 基準線の位置。数値または `CategoryField` と整合する値を渡します。 |
| `Color` | `string` | `"#00E396"` | 線の色 (`#RRGGBB`)。ラベル背景にも使用されます。 |
| `Label` | `string?` | `null` | ラベル文字列。指定した場合のみラベル表示。文字色は背景色のコントラストから自動 (黒/白) で決定。 |
| `IsDashed` | `bool` | `false` | `true` で破線。`false` で実線。 |

### `AddAnnotation(name, ChartAnnotation)`

名前付きでアノテーションを追加します。同じ名前で追加すると上書きされます。

```csharp
var a = new ChartAnnotation();
a.Axis = AnnotationAxis.X;
a.Value = 300;
a.Color = "#ff0000";
a.Label = "threshold";
a.IsDashed = true;
ApexChart5.AddAnnotation("threshold", a);
```

### `RemoveAnnotation(name)` / `ClearAnnotation()`

```csharp
ApexChart5.RemoveAnnotation("threshold");
ApexChart5.ClearAnnotation();
```

## スクリプトから扱えるプロパティ

| プロパティ | 型 | 説明 |
| --- | --- | --- |
| `AllowLoad` | `bool` | `false` の場合 `Reload()` を呼んでもデータ取得しません。初期表示を抑制したい場合に使用します。 |
| `Options` | `ApexChartOptions<SeriesData>` | Blazor-ApexCharts のオプションオブジェクト。詳細な見た目調整に直接アクセスできます。 |
| `Series` | `List<Series>` | 描画対象の系列 (読み取り専用)。 |
| `SeriesData` | `List<SeriesData>` | 取得済みのデータ (読み取り専用)。 |

> ※ `Options` はフィールドの初期化時に設定されます (色・凡例・軸ラベルの書式・グリッド・ツールバー/ツールチップの無効化など)。初期化の後 (`OnAfterInitialization` など) に書き換えてください。`Reload()` では上書きされません。
> ※ `AddAnnotation` / `RemoveAnnotation` / `ClearAnnotation` を呼ぶと `Options.Annotations` の X 軸 / Y 軸の内容は作り直されます。`Options.Annotations` に直接追加した基準線は消えるため、基準線は `AddAnnotation` で追加してください。

## 集計チャート

集計チャート 3 種 (`ApexAggregateChartField`) のスクリプト API です。基準線 (`AddAnnotation` / `RemoveAnnotation` / `ClearAnnotation`)・`AllowLoad`・`Options` は上と同じです。

### `Show(aggregator)` / `Show(aggregator, categoryCount)`

スクリプトで組んだ `ModuleAggregator` の集計を表示します。設計の定義より優先します。

- `Show(agg)`: 軸は全部カテゴリになります (複数なら「A / B」)
- `Show(agg, categoryCount)`: 軸の先頭 `categoryCount` 個をカテゴリに、残りを系列の分割に使います
- 集計円チャートは系列を分けないので、軸は全部カテゴリ・値は先頭の 1 つだけを描きます
- 値の見た目 (種類・色) は設計の系列の同じ番号を使います (無ければ棒・既定の色)

```csharp
// 担当 × 状態 の件数を、担当をカテゴリ・状態を系列にして表示
var agg = new ModuleAggregator<Sale>();
agg.GroupBy(m => m.Rep);
agg.GroupBy(m => m.Status);
agg.Count();
SalesChart.Show(agg, 1);
```

### `Reload()` / `SetAdditionalCondition(searcher)`

`Reload()` は集計し直します。`SetAdditionalCondition` は追加の条件を設計の条件 (または `Show` の定義の条件) と AND にして、**その場で集計し直します** (従来のチャートと違い、続けて `Reload()` を呼ぶ必要はありません)。

### `LoadError`

集計に失敗したときの文言です (成功なら空)。

## 注意

- スクリプトでの名前は `Reload` / `SetAdditionalCondition` です (C# 側のメソッド名は `ReloadAsync` / `SetAdditionalConditionAsync`)。
- スクリプトから `AnnotationAxis` / `ChartAnnotation` を使用するには、`ApexChartsClientInitializer.Initialize(this)` (Client.Shared) と `ApexChartsDesignerInitializer.Initialize(BlazorRuntime)` (Designer) で型登録が行われている必要があります。
- イベントハンドラ系のプロパティ (`OnQueryChangedAsync` / `OnSearchDataChangedAsync`) は `[ScriptHide]` でスクリプト公開対象外です。
