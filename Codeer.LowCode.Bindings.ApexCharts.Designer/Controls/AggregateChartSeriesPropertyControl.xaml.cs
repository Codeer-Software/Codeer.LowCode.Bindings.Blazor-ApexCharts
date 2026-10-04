using System.Windows;
using System.Windows.Controls;
using Codeer.LowCode.Bindings.ApexCharts.Designer.ViewModels;
using Codeer.LowCode.Bindings.ApexCharts.Models;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.Repository.Design;

namespace Codeer.LowCode.Bindings.ApexCharts.Designer.Controls
{
    public partial class AggregateChartSeriesPropertyControl : UserControl, ICustomPropertyControl
    {
        private AggregateChartSeries _value = new();
        private Action<bool> _completion = _ => { };
        private AggregateChartSeriesViewModel _dataContext = null!;

        public object? Value => _value;

        public AggregateChartSeriesPropertyControl()
        {
            InitializeComponent();
        }

        public void Initialize(CustomPropertyItemInfo propertyItemInfo, object? value, Action<bool> completion)
        {
            _value = (value as AggregateChartSeries) ?? new();
            _completion = completion;
            DataContext = _dataContext = new AggregateChartSeriesViewModel(propertyItemInfo.DesignData, propertyItemInfo.FieldDesign, _value);
        }

        private void OkClick(object sender, RoutedEventArgs e) => _completion(true);

        private void CancelClick(object sender, RoutedEventArgs e) => _completion(false);

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: AggregateSeriesViewModel series }) _dataContext.Remove(series.Model);
        }

        private void AddItem(object sender, RoutedEventArgs e) => _dataContext.Add();
    }
}
