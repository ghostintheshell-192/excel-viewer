// Simple RelayCommand implementation
using System.Windows.Input;

namespace ExcelViewer.UI.Avalonia.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> execute;
        private readonly Func<bool>? canExecute;

        public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                await execute();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Func<T, bool>? canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => canExecute?.Invoke((T)parameter!) ?? true;

        public void Execute(object? parameter) => execute((T)parameter!);

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }


}


