using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Designer.ViewModels
{
    public class ChartSeriesViewModel : INotifyPropertyChanged
    {
        private readonly DesignData _designData;
        private readonly ApexChartFieldDesign _design;
        private readonly ChartSeries _value;

        public ChartSeriesViewModel(DesignData designData, ApexChartFieldDesign design, ChartSeries value)
        {
            _designData = designData;
            _design = design;
            _value = value;
            foreach (var series in value.Series)
            {
                Series.Add(new SeriesViewModel(this, series));
            }
        }

        public ObservableCollection<SeriesViewModel> Series { get; } = [];
        public IEnumerable<string> NameCandidates => GetFields();
        public IEnumerable<string> TypeCandidates => GetSeriesTypes();

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Add(Series model)
        {
            Series.Add(new SeriesViewModel(this, model));
            _value.Series.Add(model);
        }

        public void Remove(Series model)
        {
            var series = Series.FirstOrDefault(e => e.Model == model);
            if (series == null) return;

            Series.Remove(series);
            _value.Series.Remove(model);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private IEnumerable<string> GetFields()
        {
            var moduleName = _design.SearchCondition.ModuleName;
            var module = _designData.Modules.Find(moduleName);
            var fieldList = module?.Fields.Where(IsCandidateType).Select(e => e.Name);
            return fieldList ?? [];
        }

        private IEnumerable<string> GetSeriesTypes()
        {
            yield return SeriesType.Area.ToString();
            yield return SeriesType.Bar.ToString();
            yield return SeriesType.Heatmap.ToString();
            yield return SeriesType.Line.ToString();
            yield return SeriesType.Scatter.ToString();
        }

        private bool IsCandidateType(FieldDesignBase field) => field is NumberFieldDesign;
    }
}
