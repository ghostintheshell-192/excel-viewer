// Enhanced RelayCommand implementation with global error handling
using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace ExcelViewer.UI.Avalonia.Commands
{
    /// <summary>
    /// RelayCommand with built-in error handling to prevent unhandled exceptions from crashing the app.
    /// Provides a global safety net for all command executions.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private readonly ILogger? _logger;
        private readonly Func<Exception, Task>? _errorHandler;

        /// <summary>
        /// Creates a RelayCommand with optional error handling
        /// </summary>
        /// <param name="execute">The async method to execute</param>
        /// <param name="canExecute">Optional predicate to determine if command can execute</param>
        /// <param name="logger">Optional logger for error reporting</param>
        /// <param name="errorHandler">Optional custom error handler (if null, uses default)</param>
        public RelayCommand(
            Func<Task> execute,
            Func<bool>? canExecute = null,
            ILogger? logger = null,
            Func<Exception, Task>? errorHandler = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _logger = logger;
            _errorHandler = errorHandler;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                await _execute();
            }
            catch (OperationCanceledException)
            {
                // User cancelled - this is normal operation, just log at info level
                _logger?.LogInformation("Command execution cancelled by user");
                // Don't show error to user - cancellation is expected behavior
            }
            catch (Exception ex)
            {
                // Log the error
                _logger?.LogError(ex, "Unhandled exception in command execution");

                // Use custom error handler if provided, otherwise just log
                if (_errorHandler != null)
                {
                    try
                    {
                        await _errorHandler(ex);
                    }
                    catch (Exception handlerEx)
                    {
                        // Error handler itself failed - just log, don't crash
                        _logger?.LogError(handlerEx, "Error handler failed while handling exception");
                    }
                }

                // If we reach here without crashing, the safety net worked!
                // The exception is logged and optionally handled, but the app continues running
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Generic RelayCommand with parameter support and built-in error handling
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;
        private readonly ILogger? _logger;
        private readonly Action<Exception>? _errorHandler;

        public RelayCommand(
            Action<T> execute,
            Func<T, bool>? canExecute = null,
            ILogger? logger = null,
            Action<Exception>? errorHandler = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _logger = logger;
            _errorHandler = errorHandler;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T)parameter!) ?? true;

        public void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _execute((T)parameter!);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation("Command execution cancelled by user");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unhandled exception in generic command execution");

                if (_errorHandler != null)
                {
                    try
                    {
                        _errorHandler(ex);
                    }
                    catch (Exception handlerEx)
                    {
                        _logger?.LogError(handlerEx, "Error handler failed in generic command");
                    }
                }
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }


}


