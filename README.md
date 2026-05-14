# Codeer.LowCode.Bindings.Blazor-ApexCharts

[Codeer.LowCode.Blazor](https://www.nuget.org/packages/Codeer.LowCode.Blazor) に [Blazor-ApexCharts](https://github.com/joadan/Blazor-ApexCharts) ベースのチャートコンポーネントを追加するライブラリです。
モジュール上のフィールドを系列として束ね、`SearchCondition` で取得した子モジュールデータをそのままチャートにバインドできます。

- ライセンス: MIT
- 対象フレームワーク: .NET 8 (Designer は `net8.0-windows`)
- NuGet:
  - [`Codeer.LowCode.Bindings.Blazor-ApexCharts`](https://www.nuget.org/packages/Codeer.LowCode.Bindings.Blazor-ApexCharts)
  - [`Codeer.LowCode.Bindings.Blazor-ApexCharts.Designer`](https://www.nuget.org/packages/Codeer.LowCode.Bindings.Blazor-ApexCharts.Designer)

## 目次

- [インストール](#インストール)
- [使用方法](#使用方法)
- [カスタムコントロール](#カスタムコントロール)
- [スクリプト API](#スクリプト-api)
- [Example](#example)

## インストール

### 1. パッケージのインストール

LowCodeApp 側の各プロジェクトに、用途別に NuGet からパッケージをインストールしてください。

| プロジェクト | パッケージ |
| --- | --- |
| `LowCodeApp.Client.Shared` | `Codeer.LowCode.Bindings.Blazor-ApexCharts` |
| `LowCodeApp.Server` | `Codeer.LowCode.Bindings.Blazor-ApexCharts` |
| `LowCodeApp.Designer` | `Codeer.LowCode.Bindings.Blazor-ApexCharts.Designer` |

### 2. 初期化コードの追加

以下の3か所に初期化コードを追加します。

#### LowCodeApp.Client.Shared

`Services/AppInfoService.cs` の `AppInfoService` コンストラクタに以下を追加します。
`ApexChartField` で使用するスクリプト型 (`AnnotationAxis` / `ChartAnnotation`) を登録します。

```csharp
ApexChartsClientInitializer.Initialize(this);
```

#### LowCodeApp.Server

`Program.cs` に以下を追加します。アセンブリのロードのみを行います。

```csharp
ApexChartsServerInitializer.Initialize();
```

#### LowCodeApp.Designer

`App.xaml.cs` の起動処理に以下を追加します。
スクリプト型登録に加え、`ChartSeries` プロパティの編集ダイアログ (WPF UserControl) を Designer に登録します。

```csharp
ApexChartsDesignerInitializer.Initialize(BlazorRuntime);
```

> `BlazorRuntime` を渡さない `Initialize()` 単体オーバーロードは Obsolete です。引数ありの方を使ってください。スコープ付き CSS の読み込みに必要です。

## 使用方法

Designer のフィールド追加メニューから、用途に応じて次の3種のフィールドを配置できます。

1. データ取得元の `Module` を `SearchCondition.ModuleName` で指定
2. X軸のカテゴリにする `CategoryField` (必要に応じて `CategoryFormat`) を指定
3. `Series` (横棒チャート / 通常チャート) または `SeriesField` (Donut/Pie/PolarArea) で値となる数値フィールドを指定

`SearchCondition.ModuleName` が未設定の場合、デザインモードでは "ModuleName is not set" バナーが表示されます。

## カスタムコントロール

このライブラリでは次の3種類のフィールドが提供されます。詳細は各ページを参照してください。

| フィールド | 用途 | 詳細 |
| --- | --- | --- |
| `ApexChartFieldDesign` | Bar / Line / Area / Heatmap / Scatter などの一般的なチャート (複数系列・型混在可) | [docs/ApexChart.md](docs/ApexChart.md) |
| `ApexHBarChartFieldDesign` | 横棒 (Horizontal Bar) チャート | [docs/ApexHBarChart.md](docs/ApexHBarChart.md) |
| `ApexRadialChartFieldDesign` | Donut / Pie / PolarArea | [docs/ApexRadialChart.md](docs/ApexRadialChart.md) |

3種に共通するプロパティ (`SearchCondition` / `DisplayName` / `CategoryField` / `CategoryFormat` / `SeriesFractionDigits` / `ShowLegend`) は [docs/ApexChart.md](docs/ApexChart.md#共通プロパティ) にまとめています。

## スクリプト API

`ApexChartField` (3種のフィールドはすべてこの実装) はスクリプトから操作できます。
リロード・追加検索条件・アノテーション（基準線）を扱えます。

詳細は [docs/Scripting.md](docs/Scripting.md) を参照してください。

簡単な例:

```csharp
void DetailLayoutDesign_OnAfterInitialization()
{
    var a = new ChartAnnotation();
    a.Axis = AnnotationAxis.X;
    a.Value = 300;
    a.Color = "#ff0000";
    a.Label = "threshold";
    a.IsDashed = true;
    ApexChart5.AddAnnotation("threshold", a);
}
```

## Example

`Example/` 配下に、各チャート種を一通り配置した動作確認用 LowCode アプリが含まれています。

- `Example/DesignData/App/Modules/Chart.mod.json` — 各チャート種の `DesignData` サンプル
- `Example/DesignData/App/Modules/Chart.mod.cs` — `AddAnnotation` を用いたスクリプト例
- `Example/DesignData/apexchart_sample.db` — 動作用 SQLite サンプル

ソリューションを開き、`LowCodeApp.Designer` または `LowCodeApp.Server` を起動して挙動を確認できます。
