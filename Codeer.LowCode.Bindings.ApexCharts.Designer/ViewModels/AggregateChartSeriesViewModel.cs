using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Codeer.LowCode.Bindings.ApexCharts.Designer.Interop;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Designer.ViewModels
{
    /// <summary>集計チャート・集計横棒チャートの系列の編集 (集計方法 / 項目 / 見出し / 種類 / 色)。</summary>
    public class AggregateChartSeriesViewModel
    {
        private readonly DesignData _designData;
        private readonly FieldDesignBase _design;
        private readonly AggregateChartSeries _value;

        public AggregateChartSeriesViewModel(DesignData designData, FieldDesignBase design, AggregateChartSeries value)
        {
            _designData = designData;
            _design = design;
            _value = value;
            foreach (var series in value.Series) Series.Add(new AggregateSeriesViewModel(this, series));
        }

        public ObservableCollection<AggregateSeriesViewModel> Series { get; } = [];

        /// <summary>項目の候補: 元モジュールの数値の項目。</summary>
        public IEnumerable<string> NameCandidates
        {
            get
            {
                var moduleName = (_design as ISearchResultsViewFieldDesign)?.SearchCondition.ModuleName ?? string.Empty;
                return _designData.Modules.Find(moduleName)?.Fields.Where(e => e is NumberFieldDesign).Select(e => e.Name).ToList() ?? [];
            }
        }

        public IEnumerable<string> FunctionCandidates => Enum.GetValues<ChartAggregateFunction>().Select(e => e.GetDisplayName()).ToList();

        //集計では行をそのまま点にする散布図は使わない
        public IEnumerable<string> TypeCandidates =>
        [
            SeriesType.Bar.GetDisplayName(),
            SeriesType.Line.GetDisplayName(),
            SeriesType.Area.GetDisplayName(),
            SeriesType.Heatmap.GetDisplayName(),
        ];

        /// <summary>横棒は種類を選べない (棒に揃える)。</summary>
        public bool IsTypeEditable => _design is not ApexAggregateHBarChartFieldDesign;

        public void Add()
        {
            var model = new AggregateSeries { Function = ChartAggregateFunction.Sum, Name = NameCandidates.FirstOrDefault() ?? string.Empty, Type = SeriesType.Bar };
            Series.Add(new AggregateSeriesViewModel(this, model));
            _value.Series.Add(model);
        }

        public void Remove(AggregateSeries model)
        {
            var series = Series.FirstOrDefault(e => e.Model == model);
            if (series == null) return;
            Series.Remove(series);
            _value.Series.Remove(model);
        }
    }

    public class AggregateSeriesViewModel : INotifyPropertyChanged
    {
        private readonly AggregateChartSeriesViewModel _owner;

        public AggregateSeriesViewModel(AggregateChartSeriesViewModel owner, AggregateSeries model)
        {
            _owner = owner;
            Model = model;
            ChooseColorCommand = new DelegateCommand(OpenColorPicker);
        }

        public AggregateSeries Model { get; }

        /// <summary>集計方法 (表示名)。件数にしたら項目は空にする。</summary>
        public string Function
        {
            get => Model.Function.GetDisplayName();
            set
            {
                var function = ChartAggregateFunctionExtensions.ParseDisplayName(value);
                if (function == null || function == Model.Function) return;
                Model.Function = function.Value;
                if (Model.Function == ChartAggregateFunction.Count) Model.Name = string.Empty;
                else if (string.IsNullOrEmpty(Model.Name)) Model.Name = NameCandidates.FirstOrDefault() ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(IsNameEditable));
            }
        }

        public string Name
        {
            get => Model.Name;
            set
            {
                if (value == Model.Name) return;
                Model.Name = value;
                OnPropertyChanged();
            }
        }

        public bool IsNameEditable => Model.Function != ChartAggregateFunction.Count;

        public string Title
        {
            get => Model.Title;
            set
            {
                if (value == Model.Title) return;
                Model.Title = value;
                OnPropertyChanged();
            }
        }

        public SeriesType Type
        {
            get => Model.Type;
            set
            {
                if (value == Model.Type) return;
                Model.Type = value;
                OnPropertyChanged();
            }
        }

        public string Color
        {
            get => Model.Color;
            set
            {
                if (value == Model.Color) return;
                Model.Color = value;
                OnPropertyChanged();
            }
        }

        public ICommand ChooseColorCommand { get; }

        public IEnumerable<string> NameCandidates => _owner.NameCandidates;
        public IEnumerable<string> FunctionCandidates => _owner.FunctionCandidates;
        public IEnumerable<string> TypeCandidates => _owner.TypeCandidates;
        public bool IsTypeEditable => _owner.IsTypeEditable;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void OpenColorPicker()
        {
            var dialog = new ColorDialog { Color = HexStringToColor(Color) };
            if (dialog.ShowDialog() == true) Color = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
        }

        private static System.Windows.Media.Color HexStringToColor(string? hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length != 7 || hex[0] != '#') return Colors.Black;
            var r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            return System.Windows.Media.Color.FromRgb(r, g, b);
        }
    }
}
