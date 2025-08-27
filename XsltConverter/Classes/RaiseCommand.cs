using System.Windows.Input;

namespace XsltConverter.Classes
{
    /// <summary>
    /// Вызов команды
    /// </summary>
    public class RaiseCommand : ICommand
    {
        public Predicate<object>? CanExecuteDelegate { get; set; }
        public Action<object> ExecuteDelegate { get; set; }

        public RaiseCommand(Action<object> executeCommand, Predicate<object>? canExecuteCommand = null)
        {
            ExecuteDelegate = executeCommand;
            CanExecuteDelegate = canExecuteCommand;
        }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
                return CanExecuteDelegate(parameter);

            return true;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            ExecuteDelegate?.Invoke(parameter);
        }
    }
}
