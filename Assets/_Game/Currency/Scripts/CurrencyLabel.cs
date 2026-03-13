using TMPro;
using UnityEngine;
using Zenject;

public class CurrencyLabel : MonoBehaviour
{
    [Header("VALUES")] 
    [SerializeField] private CurrencyType _currencyType;
    
    [Header("COMPONENTS")] 
    [SerializeField] private TextMeshProUGUI _currencyText;

    private CurrencyManager _currencyManager;

    [Inject]
    private void Construct(CurrencyManager currencyManager)
    {
        _currencyManager = currencyManager;
    }

    private void OnEnable()
    {
        if (_currencyManager == null)
        {
            return;
        }

        _currencyManager.OnCurrencyChanged += OnCurrencyChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (_currencyManager == null)
        {
            return;
        }

        _currencyManager.OnCurrencyChanged -= OnCurrencyChanged;
    }

    private void OnCurrencyChanged(CurrencyType currencyType, int amount)
    {
        if (currencyType != _currencyType)
        {
            return;
        }

        SetText(amount);
    }

    private void Refresh()
    {
        SetText(_currencyManager.Get(_currencyType));
    }

    private void SetText(int amount)
    {
        if (_currencyText == null)
        {
            return;
        }

        _currencyText.text = amount.ToString();
    }
}
