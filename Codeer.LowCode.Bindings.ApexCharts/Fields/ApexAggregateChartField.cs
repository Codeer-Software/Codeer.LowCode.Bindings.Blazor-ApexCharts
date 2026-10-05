using System.Globalization;
using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Aggregation;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.Script;
using Codeer.LowCode.Blazor.Script.Internal.ScriptServices;
using SeriesType = Codeer.LowCode.Bindings.ApexCharts.Models.SeriesType;

namespace Codeer.LowCode.Bindings.ApexCharts.Fields
{
    /// <summary>
    /// 集計チャート 3 種の実行時。元モジュールの行をカテゴリでまとめてサーバーの集計 API (権限は一覧と同じ) で集計し、1 往復でグラフにする (CrossTabBuilder)。
    /// 集計定義の出どころは 設計 か、スクリプトの Show(ModuleAggregator) で渡された定義。検索フィールドの結果の表示先 (ISearchResultsViewField) になれる。
    /// </summary>
    public class ApexAggregateChartField : FieldBase<ApexAggregateChartFieldDesignBase>, ISearchResultsViewField
    {
        SearchCondition? _additionalCondition;
        AggregateCondition? _scriptCondition;
        int _scriptCategoryCount;
        AggregateChartSetting? _userSetting;
        bool _userSettingApplied;
        Func<Task> _showCustomDialog = () => Task.CompletedTask;
        List<AggregateGroup> _categoryGroups = [];
        List<AggregateGroup> _splitGroups = [];
        AggregateChartData _chart = new();
        readonly Dictionary<string, ChartAnnotation> _annotations = new();
        Guid _refreshKey = Guid.NewGuid();

        public ApexAggregateChartField(ApexAggregateChartFieldDesignBase design) : base(design) { }

        /// <summary>読み込みを止める (条件を組み立ててから Reload で読む等)。</summary>
        public bool AllowLoad { get; set; } = true;

        /// <summary>集計している元モジュール (スクリプトの定義があればそのモジュール)。</summary>
        [ScriptHide]
        public string ModuleName => _scriptCondition?.ModuleName ?? Design.SearchCondition.ModuleName;

        [ScriptHide]
        public SearchField? SearchField { get; set; }

        public ApexChartOptions<SeriesData> Options { get; } = new();

        /// <summary>描く系列 (Name は内部の鍵。見出しは GetSeriesTitle)。</summary>
        public List<Series> Series => _chart.Series.ToList();

        /// <summary>カテゴリごとの値。</summary>
        public List<SeriesData> SeriesData => _chart.Data.ToList();

        /// <summary>読み込みに失敗したときのメッセージ (成功なら空)。</summary>
        public string LoadError { get; private set; } = string.Empty;

        public bool IsLoading { get; private set; }

        public override bool IsModified => false;

        /// <summary>今使っている集計の設定 (利用者のカスタマイズがあればそれ、無ければ設計)。スクリプトの Show で渡した定義は含まない。</summary>
        internal AggregateChartSetting CurrentSetting => _userSetting ?? Design.GetSetting();

        /// <summary>利用者がカスタマイズできる状態か (設計で許可・元モジュールあり・スクリプトの Show で定義を渡していない・実行時)。</summary>
        internal bool CanCustomize => Design.CanCustomize && _scriptCondition == null
            && !string.IsNullOrEmpty(Design.SearchCondition.ModuleName) && !Services.AppInfoService.IsDesignMode;

        //カスタマイズできるチャートは、画面がブラウザの保存内容を読んで ApplyUserSettingAsync を呼ぶまで集計しない (設計の設定で 1 回集計してから読み直さない。検索フィールドの初回検索が先に来ても同じ)
        bool IsWaitingUserSetting => Design.CanCustomize && !_userSettingApplied && !Services.AppInfoService.IsDesignMode;

        /// <summary>利用者の設定 (今の設計で使えるもの。null なら設計どおり) を使って集計し直す。画面が呼ぶ。</summary>
        internal async Task ApplyUserSettingAsync(AggregateChartSetting? setting)
        {
            _userSetting = setting == null ? null : Design.NormalizeSetting(setting);
            _userSettingApplied = true;
            //値の軸の書式はヒートマップかどうかで変わる
            if (this.IsInLayout()) InitializeOptions();
            await ReloadAsync();
        }

        internal void SetShowCustomDialog(Func<Task> show) => _showCustomDialog = show;

        /// <summary>集計のカスタマイズのダイアログを開く (設計で「利用者が集計を変更できる」がオンのときだけ開く)。</summary>
        [ScriptName("ShowCustomDialog")]
        public async Task ShowCustomDialogAsync()
        {
            if (!CanCustomize) return;
            await _showCustomDialog();
        }

        [ScriptHide]
        public string RefreshKey => _refreshKey.ToString();

        /// <summary>系列の見出し (凡例に出る文字)。</summary>
        [ScriptHide]
        public string GetSeriesTitle(string seriesKey) => _chart.Titles.TryGetValue(seriesKey, out var title) ? title : seriesKey;

        [ScriptHide]
        public override async Task InitializeDataAsync(FieldDataBase? fieldDataBase)
        {
            if (!this.IsInLayout()) return;
            InitializeOptions();
            //画面に置いた検索フィールドの表示先なら、検索フィールドの初回検索が集計する (ここでも集計すると 2 回になる)
            if (HasSearchField())
            {
                Refresh();
                return;
            }
            await ReloadAsync();
        }

        //この集計チャートを結果の表示先にしている検索フィールドが画面に置かれているか
        bool HasSearchField()
            => !Services.AppInfoService.IsDesignMode
               && Module.Design.Fields.OfType<SearchFieldDesign>().Any(e => e.ResultsViewFieldName == Design.Name && Module.GetField(e.Name) is { } field && field.IsInLayout());

        [ScriptHide]
        public override FieldDataBase? GetData() => null;
        [ScriptHide]
        public override FieldSubmitData GetSubmitData() => new();
        [ScriptHide]
        public override async Task SetDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;

        /// <summary>スクリプトで組んだ集計定義を表示する。軸は全部カテゴリになる (複数なら「A / B」)。</summary>
        [ScriptName("Show")]
        public async Task ShowAsync(ModuleAggregator aggregator) => await ShowAsync(aggregator, -1);

        /// <summary>スクリプトで組んだ集計定義を表示する。軸の先頭 categoryCount 個をカテゴリに、残りを系列の分割に使う (円は分割しない)。設計の定義より優先し、追加の条件は AND になる。</summary>
        [ScriptName("Show")]
        public async Task ShowAsync(ModuleAggregator aggregator, int categoryCount)
        {
            var condition = aggregator.GetAggregateCondition().JsonClone();
            _scriptCondition = condition;
            _scriptCategoryCount = categoryCount < 0 || Design is ApexAggregateRadialChartFieldDesign ? condition.Groups.Count : Math.Min(categoryCount, condition.Groups.Count);
            await ReloadAsync();
        }

        /// <summary>追加の条件 (スクリプトの ModuleSearcher)。設計の条件と AND になり、集計し直す。</summary>
        [ScriptName("SetAdditionalCondition")]
        public async Task SetAdditionalConditionAsync(ModuleSearcher searcher)
        {
            await SetAdditionalConditionAsync(searcher.GetSearchCondition(), 0);
            await ReloadAsync();
        }

        /// <summary>追加の条件を入れるだけ (読み直しは呼び出し側。検索フィールドは続けて ReloadAsync を呼ぶ)。</summary>
        [ScriptHide]
        public Task SetAdditionalConditionAsync(SearchCondition condition, int page)
        {
            if (condition.ModuleName != ModuleName)
                throw LowCodeException.Create("{0} Invalid Module", ModuleName, condition.ModuleName);
            _additionalCondition = condition;
            return Task.CompletedTask;
        }

        [ScriptHide]
        public override async Task OnExternalFieldChangedAsync(string fieldName)
        {
            if (!this.IsInLayout()) return;
            if (GetSearchCondition().GetFieldVariableConditions().All(e => new VariableName(e.Variable).FieldName.Root != fieldName)) return;
            await ReloadAsync();
        }

        /// <summary>集計し直す。</summary>
        [ScriptName("Reload")]
        public async Task ReloadAsync()
        {
            if (!AllowLoad || IsWaitingUserSetting) return;
            if (Services.AppInfoService.IsDesignMode)
            {
                _chart = CreateDesignSample();
                Refresh();
                return;
            }
            if (string.IsNullOrEmpty(ModuleName)) return;
            var condition = GetAggregateCondition(out var categoryCount);
            if (condition.Measures.Count == 0 || condition.Groups.Count == 0) return;
            //UTC 保存の日時は見ている人 (ブラウザ) の時差で区切る
            if (condition.UtcOffsetMinutes == null) condition.UseLocalTimeZone();
            IsLoading = true;
            LoadError = string.Empty;
            NotifyStateChanged();
            try
            {
                var table = await CrossTabBuilder.BuildAsync(condition, categoryCount, Services.ModuleDataService.AggregateAsync, withTotals: false);
                //null = ホストの集計 API が失敗を返した (理由はホストが通知済み)
                if (table == null)
                {
                    _chart = new AggregateChartData();
                    LoadError = Properties.Resources.AggregateFailed;
                    return;
                }
                _categoryGroups = condition.Groups.Take(categoryCount).ToList();
                _splitGroups = condition.Groups.Skip(categoryCount).ToList();
                _chart = ChartAggregate.ToChart(table, GetStyles(condition.Measures.Count), KeyText, MeasureTitle);
            }
            catch (Exception e)
            {
                _chart = new AggregateChartData();
                LoadError = e.Message;
            }
            finally
            {
                IsLoading = false;
                Refresh();
            }
        }

        public void AddAnnotation(string name, ChartAnnotation annotation)
        {
            _annotations[name] = annotation;
            ApplyAnnotations();
        }

        public void RemoveAnnotation(string name)
        {
            if (!_annotations.Remove(name)) return;
            ApplyAnnotations();
        }

        public void ClearAnnotation()
        {
            _annotations.Clear();
            ApplyAnnotations();
        }

        /// <summary>今の集計定義とカテゴリの数 (設計 または Show で渡した定義。条件は追加の条件と AND)。</summary>
        internal AggregateCondition GetAggregateCondition(out int categoryCount)
        {
            AggregateCondition condition;
            if (_scriptCondition == null)
            {
                condition = ChartAggregate.CreateCondition(Design.SearchCondition, CurrentSetting);
                categoryCount = 1;
            }
            else
            {
                condition = _scriptCondition.JsonClone();
                categoryCount = _scriptCategoryCount;
            }
            //円は値を 1 つだけ描く
            if (Design is ApexAggregateRadialChartFieldDesign && condition.Measures.Count > 1) condition.Measures.RemoveRange(1, condition.Measures.Count - 1);
            condition.Condition = GetSearchCondition().Condition;
            return condition;
        }

        //条件の元: 設計の検索条件、または Show で渡した定義の条件
        SearchCondition GetSearchCondition()
        {
            var baseCondition = _scriptCondition == null
                ? Design.SearchCondition
                : new SearchCondition(_scriptCondition.ModuleName) { Condition = _scriptCondition.Condition };
            return baseCondition.MergeSearchCondition(_additionalCondition);
        }

        //見た目 (種類・色) を取る系列: 今の設定の系列。スクリプトの Show で定義を渡したときは設計の系列
        IReadOnlyList<AggregateSeries> StyleSeries => _scriptCondition == null ? CurrentSetting.Series : Design.GetSeries();

        //値の見た目は StyleSeries の同じ番号 (Show の定義で設計より多い値は棒・色なし)
        List<ChartAggregate.SeriesStyle> GetStyles(int count)
        {
            var series = StyleSeries;
            return Enumerable.Range(0, count).Select(i => i < series.Count
                ? new ChartAggregate.SeriesStyle(series[i].Type, series[i].Color)
                : new ChartAggregate.SeriesStyle(Design is ApexAggregateRadialChartFieldDesign radial ? radial.SeriesType : SeriesType.Bar, string.Empty)).ToList();
        }

        string KeyText(AggregateKey key, int index, bool isCategory)
        {
            var groups = isCategory ? _categoryGroups : _splitGroups;
            var group = index < groups.Count ? groups[index] : null;
            var field = group == null ? null : SourceField(group.Variable);
            return ChartAggregate.KeyText(key, group, field, Properties.Resources.Blank, CultureInfo.CurrentCulture);
        }

        string MeasureTitle(AggregateMeasure measure)
        {
            if (!string.IsNullOrEmpty(measure.Name)) return Services.AppInfoService.Localize(measure.Name);
            var function = FunctionText(measure.Function);
            if (measure.Function == AggregateFunction.Count) return function;
            var field = SourceField(measure.Variable);
            var name = field == null ? new VariableName(measure.Variable).FieldName.FullName : Services.AppInfoService.Localize(DisplayTextOf(field));
            return $"{name} {function}";
        }

        static string FunctionText(AggregateFunction function) => function switch
        {
            AggregateFunction.Count => ChartAggregateFunction.Count.GetDisplayName(),
            AggregateFunction.CountDistinct => Properties.Resources.Function_CountDistinct,
            AggregateFunction.Avg => ChartAggregateFunction.Avg.GetDisplayName(),
            AggregateFunction.Min => ChartAggregateFunction.Min.GetDisplayName(),
            AggregateFunction.Max => ChartAggregateFunction.Max.GetDisplayName(),
            _ => ChartAggregateFunction.Sum.GetDisplayName(),
        };

        static string DisplayTextOf(FieldDesignBase field)
            => field is IDisplayName d && !string.IsNullOrEmpty(d.DisplayName) ? d.DisplayName : field.Name;

        //軸・値が指す元モジュールの項目 (リンク越しも可。無ければ null)
        FieldDesignBase? SourceField(string variable)
        {
            if (string.IsNullOrEmpty(variable)) return null;
            var designData = Services.AppInfoService.GetDesignData();
            var module = designData.Modules.Find(ModuleName);
            return module == null ? null : ChartAggregate.ResolveField(designData, module, variable);
        }

        //デザイナのプレビュー用 (カテゴリ 5 つ × 設計の系列)。数字はダミー
        AggregateChartData CreateDesignSample()
        {
            var chart = new AggregateChartData();
            var series = Design.GetSeries();
            if (series.Count == 0) series = [new AggregateSeries { Function = ChartAggregateFunction.Count }];
            if (Design is ApexAggregateRadialChartFieldDesign) series = series.Take(1).ToList();
            var categoryName = string.IsNullOrEmpty(Design.CategoryField) ? "Category" : Design.CategoryField;
            for (var i = 0; i < series.Count; i++)
            {
                var key = "s" + i.ToString(CultureInfo.InvariantCulture);
                var color = string.IsNullOrEmpty(series[i].Color) ? ChartAggregate.DefaultTheme[i % ChartAggregate.DefaultTheme.Length] : series[i].Color;
                chart.Series.Add(new Series { Name = key, Color = color, Type = series[i].Type });
                chart.Titles[key] = !string.IsNullOrEmpty(series[i].Title) ? series[i].Title
                    : series[i].Function == ChartAggregateFunction.Count ? series[i].Function.GetDisplayName() : $"{series[i].Name} {series[i].Function.GetDisplayName()}";
            }
            for (var c = 0; c < 5; c++)
            {
                var data = new SeriesData { XValue = $"{categoryName} {c + 1}" };
                for (var i = 0; i < series.Count; i++) data.Data["s" + i.ToString(CultureInfo.InvariantCulture)] = (c + 1) * 10 + i * 7;
                chart.Data.Add(data);
            }
            return chart;
        }

        void Refresh()
        {
            if (Design is not ApexAggregateRadialChartFieldDesign) Options.Colors = _chart.Series.Select(s => s.Color).ToList();
            _refreshKey = Guid.NewGuid();
            NotifyStateChanged();
        }

        void InitializeOptions()
        {
            Options.Chart ??= new Chart();
            Options.Chart.Id = "a" + Guid.NewGuid().ToString().Replace("-", "");
            Options.Chart.Height = "100%";
            Options.Legend = new Legend { Position = Design.ShowLegend ? LegendPosition.Bottom : null };

            var digits = Design.GetFractionDigits();
            var heatmap = StyleSeries.FirstOrDefault()?.Type == SeriesType.Heatmap;
            var formatter = digits == null || heatmap ? null : $"function(value) {{ return Number(value).toFixed({Math.Max(0, digits.Value)}); }}";
            if (Design is ApexAggregateHBarChartFieldDesign)
            {
                Options.Xaxis = new XAxis { Labels = new XAxisLabels { Formatter = formatter } };
                Options.Yaxis = [new YAxis()];
                Options.PlotOptions ??= new PlotOptions();
                Options.PlotOptions.Bar ??= new PlotOptionsBar();
                Options.PlotOptions.Bar.Horizontal = true;
            }
            else if (Design is ApexAggregateChartFieldDesign chartDesign)
            {
                Options.Yaxis = [new YAxis { Labels = new YAxisLabels { Formatter = formatter } }];
                Options.Grid ??= new Grid();
                Options.Grid.Xaxis ??= new GridXAxis();
                Options.Grid.Xaxis.Lines ??= new Lines();
                Options.Grid.Xaxis.Lines.Show = chartDesign.ShowXAxisGrid;
                Options.Grid.Yaxis ??= new GridYAxis();
                Options.Grid.Yaxis.Lines ??= new Lines();
                Options.Grid.Yaxis.Lines.Show = chartDesign.ShowYAxisGrid;
            }

            var color = Color.Or(this.GetFontAppearance()?.Color) ?? "";
            if (!string.IsNullOrEmpty(color)) Options.Chart.ForeColor = color;
            var backgroundColor = BackgroundColor.Or(this.GetBackgroundColor()) ?? "";
            if (!string.IsNullOrEmpty(backgroundColor)) Options.Chart.Background = backgroundColor;

            Options.Annotations ??= new Annotations();
            Options.Annotations.Xaxis ??= [];
            Options.Annotations.Yaxis ??= [];

            //ツールバーは出さない。集計値を読めるようツールチップは出す (従来のチャートは既定で出さない)
            Options.Chart.Toolbar = new Toolbar { Show = false };
            Options.Tooltip = new Tooltip { Enabled = true };
        }

        void ApplyAnnotations()
        {
            Options.Annotations ??= new Annotations();
            Options.Annotations.Xaxis ??= [];
            Options.Annotations.Yaxis ??= [];
            Options.Annotations.Xaxis.Clear();
            Options.Annotations.Yaxis.Clear();
            foreach (var annotation in _annotations.Values)
            {
                var label = annotation.Label == null ? new Label() : new Label
                {
                    BorderColor = annotation.Color,
                    Style = new Style { Color = AutoColor.ContrastColor(annotation.Color), Background = annotation.Color },
                    Text = annotation.Label,
                };
                if (annotation.Axis == AnnotationAxis.X)
                    Options.Annotations.Xaxis.Add(new AnnotationsXAxis { X = annotation.Value, BorderColor = annotation.Color, StrokeDashArray = annotation.IsDashed ? null : 0, Label = label });
                else
                    Options.Annotations.Yaxis.Add(new AnnotationsYAxis { Y = annotation.Value, BorderColor = annotation.Color, StrokeDashArray = annotation.IsDashed ? null : 0, Label = label });
            }
            _refreshKey = Guid.NewGuid();
            NotifyStateChanged();
        }
    }
}
