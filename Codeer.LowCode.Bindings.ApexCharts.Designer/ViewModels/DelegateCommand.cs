using System.Windows.Input;

namespace Codeer.LowCode.Bindings.ApexCharts.Designer.ViewModels
{
    internal class DelegateCommand(Action action) : ICommand
    {
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            action();
        }
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
