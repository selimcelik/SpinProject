using System;
using System.Collections.Generic;
using Zenject;

public class CurrencyManager : IInitializable
{
    public event Action<CurrencyType, int> OnCurrencyChanged;

    private readonly Dictionary<CurrencyType, int> _balances = new Dictionary<CurrencyType, int>();

    public void Initialize()
    {
        _balances[CurrencyType.Coin] = 40;
        NotifyChanged(CurrencyType.Coin);
    }

    public void Add(CurrencyType type, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _balances[type] = Get(type) + amount;
        NotifyChanged(type);
    }

    public bool Remove(CurrencyType type, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        int currentAmount = Get(type);

        if (currentAmount < amount)
        {
            return false;
        }

        _balances[type] = currentAmount - amount;
        NotifyChanged(type);
        return true;
    }

    public int Get(CurrencyType type)
    {
        return _balances.TryGetValue(type, out int amount) ? amount : 0;
    }

    private void NotifyChanged(CurrencyType type)
    {
        OnCurrencyChanged?.Invoke(type, Get(type));
    }
}

public enum CurrencyType
{
    Coin = 0,
}
