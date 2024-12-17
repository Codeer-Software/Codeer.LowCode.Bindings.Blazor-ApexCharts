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
        ICustomPropertyControl<ApexChartFieldDesign, ChartSeries>
    {
        private ChartSeries? _value;
        private Action<bool?, object?>? _completion;
        private ChartSeriesViewModel _dataContext = null!;

        public ChartSeriesPropertyControl()
        {
            InitializeComponent();
        }

        public void OpenDialog(DesignData designData, ApexChartFieldDesign design, ChartSeries? value,
            Action<bool?, object?> completion)
        {
            _value = value;
            _completion = completion;
            DataContext = _dataContext = new ChartSeriesViewModel(designData, design, value!);
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            _completion!(true, _value);
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            _completion!(false, null);
        }

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
