using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TreaYT.Common;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected AsyncRelayCommand CreateAsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
    {
        return new AsyncRelayCommand(execute, canExecute);
    }

    protected AsyncRelayCommand<T> CreateAsyncRelayCommand<T>(Func<T, Task> execute, Func<T, bool> canExecute = null)
    {
        return new AsyncRelayCommand<T>(execute, canExecute);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Release managed resources
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}