using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Codeer.LowCode.Bindings.ApexCharts.Designer.Interop;
using Codeer.LowCode.Bindings.ApexCharts.Models;

namespace Codeer.LowCode.Bindings.ApexCharts.Designer.ViewModels
{
    public class SeriesViewModel : INotifyPropertyChanged
    {
        private readonly ChartSeriesViewModel _viewModel;

        public SeriesViewModel(ChartSeriesViewModel viewModel, Series model)
        {
            _viewModel = viewModel;
            Model = model;
            ChooseColorCommand = new DelegateCommand(OpenColorPicker);
        }

        public Series Model { get; }

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

        public global::ApexCharts.SeriesType Type
        {
            get => Model.Type;
            set
            {
                if (value == Model.Type) return;
                Model.Type = value;
                OnPropertyChanged();
            }
        }

        public ICommand ChooseColorCommand { get; set; }

        public IEnumerable<string> NameCandidates => _viewModel.NameCandidates;
        public IEnumerable<string> TypeCandidates => _viewModel.TypeCandidates;

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

        private void OpenColorPicker()
        {
            var dialog = new ColorDialog
            {
                Color = HexStringToColor(Color)
            };
            if (dialog.ShowDialog() == true)
            {
                Color = ColorToHexString(dialog.Color);
            }
        }

        private string ColorToHexString(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        private Color HexStringToColor(string? hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                return Colors.Black;
            }
            if (hex.Length != 7 || hex[0] != '#')
            {
                return Colors.Black;
            }

            var r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            return System.Windows.Media.Color.FromRgb(r, g, b);
        }
    }
}
