using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.Script;
using Codeer.LowCode.Blazor.Script.Internal.ScriptServices;

namespace Codeer.LowCode.Bindings.ApexCharts.Fields
{
    public class ApexChartField : FieldBase<ApexChartFieldDesignBase>, ISearchResultsViewField
    {
        private static readonly string[] DefaultTheme = ["#008FFB", "#00E396", "#FEB019", "#FF4560", "#775DD0"];

        private List<SeriesData> _data = [];
        private readonly List<Series> _series;
        private SearchCondition? _additionalCondition;

        [ScriptHide]
        public Func<SearchCondition?, Task> OnQueryChangedAsync { get; set; } = _ => Task.CompletedTask;

        [ScriptHide]
        public Func<Task> OnSearchDataChangedAsync { get; set; } = async () => await Task.CompletedTask;

        public bool AllowLoad { get; set; } = true;

        [ScriptHide]
        public string ModuleName => Design?.SearchCondition.ModuleName ?? string.Empty;

        public ApexChartOptions<SeriesData> Options { get; }

        public List<Series> Series => (Services.AppInfoService.IsDesignMode ? GetDesignSeries() : _series).ToList();

        public List<SeriesData> SeriesData =>
            (Services.AppInfoService.IsDesignMode ? GetDesignSeriesData() : _data).ToList();

        public List<SeriesData> SingleSeriesData => GetSingleSeriesData();

        public override bool IsModified => false;

        public ApexChartField(ApexChartFieldDesignBase design) : base(design)
        {
            var series = NormalizeSeries(design);
            var isHeatmap = series.FirstOrDefault()?.Type == SeriesType.Heatmap;
            _series = series.Where(s => (s.Type == SeriesType.Heatmap) == isHeatmap).ToList();

            var colors = _series.Select((t, i) => string.IsNullOrEmpty(t.Color) ? DefaultTheme[i % 5] : t.Color).ToList();

            Options = new ApexChartOptions<SeriesData>();
            Options.Chart.Id = "a" + Guid.NewGuid().ToString().Replace("-", "");
            if (Design is ApexChartFieldDesign)
            {
                Options.Colors = colors;
            }

            Options.Legend = new Legend { Position = Design.ShowLegend ? LegendPosition.Bottom : null };
            Options.Yaxis =
            [
                new YAxis
                {
                    Labels = new YAxisLabels
                    {
                        Formatter = Design.SeriesType != SeriesType.Heatmap
                            ? $"function(value) {{ return Number(value).toFixed({Math.Max(0, Design.SeriesFractionDigits)}); }}"
                            : null
                    },
                }
            ];
            Options.Chart ??= new Chart();
            Options.Chart.Height = "100%";

            if (Design is ApexChartFieldDesign chartDesign)
            {
                if (chartDesign.FullWidthBar)
                {
                    Options.PlotOptions ??= new PlotOptions();
                    Options.PlotOptions.Bar ??= new PlotOptionsBar();
                    Options.PlotOptions.Bar.ColumnWidth = "101%";
                }

                Options.Grid ??= new Grid();
                Options.Grid.Xaxis ??= new GridXAxis();
                Options.Grid.Xaxis.Lines ??= new Lines();
                Options.Grid.Xaxis.Lines.Show = chartDesign.ShowXAxisGrid;

                Options.Grid.Yaxis ??= new GridYAxis();
                Options.Grid.Yaxis.Lines ??= new Lines();
                Options.Grid.Yaxis.Lines.Show = chartDesign.ShowYAxisGrid;
            }

            if (_series.Any(s => s.Type == SeriesType.Scatter))
            {
                Options.Markers ??= new Markers();
                Options.Markers.Size = new Size(5, 5, 5, 5);
            }
        }

        private static List<Series> NormalizeSeries(ApexChartFieldDesignBase design)
        {
            var series = design switch
            {
                ApexRadialChartFieldDesign radialDesign => radialDesign.SeriesField == null
                    ? []
                    : [new Series { Name = radialDesign.SeriesField, Type = radialDesign.SeriesType }],
                ApexChartFieldDesign chartDesign => chartDesign.Series.Series,
                _ => []
            };
            return series;
        }

        [ScriptHide]
        public override async Task InitializeDataAsync(FieldDataBase? fieldDataBase)
        {
            if (this.IsInLayout()) await ReloadAsync();
        }

        [ScriptName("SetAdditionalCondition")]
        public async Task SetAdditionalConditionAsync(ModuleSearcher searcher)
            => await SetAdditionalConditionAsync(searcher.GetSearchCondition(), 0);

        [ScriptHide]
        public async Task SetAdditionalConditionAsync(SearchCondition condition, int page)
        {
            if (condition.ModuleName != Design.SearchCondition.ModuleName)
                throw LowCodeException.Create("{0} Invalid Module", Design.SearchCondition.ModuleName,
                    condition.ModuleName);
            _additionalCondition = condition;
            await OnQueryChangedAsync(GetSearchCondition());
        }

        [ScriptHide]
        public override FieldDataBase? GetData() => null;

        [ScriptHide]
        public override FieldSubmitData GetSubmitData() => new();

        [ScriptHide]
        public override async Task SetDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;

        [ScriptHide]
        public override async Task OnExternalFieldChangedAsync(string fieldName)
        {
            if (!this.IsInLayout()) return;
            var searchCondition = GetSearchCondition();
            if (searchCondition.GetFieldVariableConditions()
                .All(e => new VariableName(e.Variable).FieldName.Root != fieldName)) return;
            await ReloadAsync();
        }

        [ScriptName("Reload")]
        public async Task ReloadAsync()
        {
            if (!AllowLoad) return;
            SetSeriesData(await this.GetChildModulesAsync(GetSearchCondition(), ModuleLayoutType.None));
        }

        public void SetSeriesData(List<Module> items)
        {
            _data = items
                .Select((e, i) => new SeriesData
                {
                    XValue = Format(GetValue(e.GetField(Design.CategoryField))) ?? i.ToString(),
                    Data = e.GetFields().OfType<NumberField>().ToDictionary(x => x.Design.Name, x => x.Value),
                })
                .ToList();
            NotifyStateChanged();
        }

        private SearchCondition GetSearchCondition()
            => Design.SearchCondition.MergeSearchCondition(_additionalCondition);

        private List<Series> GetDesignSeries() =>
        [
            new() { Name = "ChartA", Type = _series.ElementAtOrDefault(0)?.Type ?? SeriesType.Bar },
            new() { Name = "ChartB", Type = _series.ElementAtOrDefault(0)?.Type ?? SeriesType.Bar },
        ];

        private List<SeriesData> GetDesignSeriesData() => Enumerable.Range(1, 10)
            .Select(d => Tuple.Create(Math.Sqrt(d) * 3, Math.Cos(d * 9 / 57.2958) * 10))
            .Select((data, i) => new SeriesData
            {
                XValue = i,
                Data = new Dictionary<string, decimal?>
                        { { "ChartA", (decimal)data.Item1 }, { "ChartB", (decimal)data.Item2 } }
            }
            ).ToList();

        private object? GetValue(FieldBase? fieldBase)
        {
            if (fieldBase is TextField textField) return textField.Value;
            if (fieldBase is NumberField numberField) return numberField.Value;
            if (fieldBase is DateTimeField dateTime) return dateTime.Value;
            if (fieldBase is DateField date) return date.Value;
            return null;
        }

        private List<SeriesData> GetSingleSeriesData()
        {
            var data = SeriesData.FirstOrDefault();
            if (data is null) return [];
            return Series.Select(key => new SeriesData
            {
                Data = new Dictionary<string, decimal?> { { "XValue", data.Data[key.Name] } }
            }).ToList();
        }

        private object? Format(object? value)
        {
            if (value is null) return value;
            if (string.IsNullOrEmpty(Design.CategoryFormat)) return value;
            return value.GetType().GetMethod("ToString", [typeof(string)])?.Invoke(value, [Design.CategoryFormat]) ??
                   value;
        }


    }
}
