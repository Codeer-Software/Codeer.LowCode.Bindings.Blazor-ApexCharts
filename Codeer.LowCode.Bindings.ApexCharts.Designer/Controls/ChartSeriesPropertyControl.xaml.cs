using System.Windows;
using System.Windows.Controls;
using Codeer.LowCode.Bindings.ApexCharts.Designer.ViewModels;
using Codeer.LowCode.Bindings.ApexCharts.Designs;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Designer.Controls
{
    public partial class ChartSeriesPropertyControl : UserControl,
        ICustomPropertyControl
    {
        private ChartSeries _value = new();
        private Action<bool> _completion = _ => { };
        private ChartSeriesViewModel _dataContext = null!;

        public object? Value => _value;

        public ChartSeriesPropertyControl()
        {
            InitializeComponent();
        }

        public void Initialize(CustomPropertyItemInfo propertyItemInfo, object? value,
            Action<bool> completion)
        {
            _value = (value as ChartSeries) ?? new();
            _completion = completion;
            DataContext = _dataContext = new ChartSeriesViewModel(propertyItemInfo.DesignData, (ApexChartFieldDesign)propertyItemInfo.FieldDesign, _value);
        }

        private void OkClick(object sender, RoutedEventArgs e)
            => _completion!(true);

        private void CancelClick(object sender, RoutedEventArgs e)
            => _completion!(false);

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: SeriesViewModel series })
            {
                _dataContext.Remove(series.Model);
            }
        }

        private void AddItem(object sender, RoutedEventArgs e)
        {
            _dataContext.Add(new Series());
        }
    }
}
