using System.ComponentModel;
using System.Runtime.CompilerServices;
using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts.Models;

namespace Codeer.LowCode.Bindings.ApexCharts.Designer.ViewModels
{
    public class SeriesViewModel(ChartSeriesViewModel viewModel, Series model) : INotifyPropertyChanged
    {
        public Series Model { get; } = model;

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

        public IEnumerable<string> NameCandidates => viewModel.NameCandidates;
        public IEnumerable<string> TypeCandidates => viewModel.TypeCandidates;

        public event PropertyChangedEventHandler? PropertyChanged;

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
    }
}
