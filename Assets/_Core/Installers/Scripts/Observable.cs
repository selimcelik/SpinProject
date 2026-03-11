using System;
using System.Collections.Generic;

public class Observable<T>
{
    public event Action<T> OnChanged;

    private T _value;

    public T Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
            {
                return;
            }

            _value = value;
            OnChanged?.Invoke(_value);
        }
    }

    public Observable()
    {
    }

    public Observable(T defaultValue)
    {
        _value = defaultValue;
    }

    public void Notify()
    {
        OnChanged?.Invoke(_value);
    }
}
