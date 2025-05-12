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
- 
#### LowCodeApp.Client.Shared

`Services/AppInfoService.cs` のAppInfoServiceコンストラクタに以下のコードを追加してください。

```csharp
ApexChartsClientInitializer.Initialize(this);
```

#### LowCodeApp.Server

`Program.cs` に以下のコードを追加してください。

```csharp
ApexChartsServerInitializer.Initialize();
```

#### LowCodeApp.Designer

`App.xaml.cs` に以下のコードを追加してください。

```csharp
ApexChartsDesignerInitializer.Initialize();
```

## 使用方法

DesignerからApexChartが配置できるようになっています。

## カスタムコントロール

このライブラリでは次のカスタムコントロールが提供されています。

- ApexChart

## ApexChart

ApexChartライブラリを使用してデータをグラフで表示するコンポーネントです。
