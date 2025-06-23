using System.Windows.Input;

namespace TreaYT.Common;

public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool> _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke() ?? true);
    }

    public async void Execute(object parameter)
    {
        await ExecuteAsync();
    }

    public async Task ExecuteAsync()
    {
        if (CanExecute(null))
        {
            try
            {
                _isExecuting = true;
                NotifyCanExecuteChanged();
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                NotifyCanExecuteChanged();
            }
        }
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class AsyncRelayCommand<T> : ICommand
{
    private readonly Func<T, Task> _execute;
    private readonly Func<T, bool> _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke(parameter is T t ? t : default) ?? true);
    }

    public async void Execute(object parameter)
    {
        await ExecuteAsync(parameter is T t ? t : default);
    }

    public async Task ExecuteAsync(T parameter)
    {
        if (CanExecute(parameter))
        {
            try
            {
                _isExecuting = true;
                NotifyCanExecuteChanged();
                await _execute(parameter);
            }
            finally
            {
                _isExecuting = false;
                NotifyCanExecuteChanged();
            }
        }
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}