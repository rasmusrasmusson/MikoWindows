
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MikoMe.Helpers;

public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? name=null)
    {
        if (!Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    protected void Raise([CallerMemberName] string? name=null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
