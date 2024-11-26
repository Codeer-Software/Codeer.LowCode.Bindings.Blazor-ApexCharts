# Codeer.LowCode.Bindings.Blazor-ApexCharts

Codeer.LowCode.BlazorにApexChartsコンポーネントを追加するためのライブラリです。

## インストール

### パッケージのインストール

LowCodeApp.Client.Shared プロジェクトにNuGetから次のパッケージをインストールしてください。

- Codeer.LowCode.Bindings.Blazor-ApexCharts

### コードの修正

ライブラリの使用に必要なコードを以下のプロジェクトにそれぞれ追加する必要があります。

- LowCodeApp.Server
- LowCodeApp.Designer

#### LowCodeApp.Server

`Program.cs` に以下のコードを追加してください。

```csharp
typeof(ApexChartFieldDesign).ToString();
typeof(SeriesType).ToString();
```

#### LowCodeApp.Designer

`App.xaml.cs` に以下のコードを追加してください。

```csharp
typeof(ApexChartFieldDesign).ToString();
```

## 使用方法

DesignerからApexChartが配置できるようになっています。

## カスタムコントロール

このライブラリでは次のカスタムコントロールが提供されています。

- ApexChart

## ApexChart

ApexChartライブラリを使用してデータをグラフで表示するコンポーネントです。