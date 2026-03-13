using Popup;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class DeathPopup : IPopup
{

    [Header("COMPONENTS")]
    [SerializeField] private Button _giveUpButton;
    [SerializeField] private Button _reviveButton;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Image _itemIcon;

    [Header("READONLY")]
    [ShowInInspector, ReadOnly] private SpinWaveItemData _waveItemData;

    private CurrencyManager _currencyManager;
    private ItemManager _itemManager;
    private SpinManager _spinManager;

    [Inject]
    private void Construct(CurrencyManager currencyManager, ItemManager itemManager,SpinManager spinManager)
    {
        _currencyManager = currencyManager;
        _itemManager = itemManager;
        _spinManager = spinManager;
    }

    private void OnEnable()
    {
        if (_giveUpButton != null)
        {
            _giveUpButton.onClick.AddListener(OnGiveUpClicked);
        }

        if (_reviveButton != null)
        {
            _reviveButton.onClick.AddListener(OnReviveClicked);
        }
    }

    private void OnDisable()
    {
        if (_giveUpButton != null)
        {
            _giveUpButton.onClick.RemoveListener(OnGiveUpClicked);
        }

        if (_reviveButton != null)
        {
            _reviveButton.onClick.RemoveListener(OnReviveClicked);
        }
    }

    public void Setup(SpinWaveItemData waveItemData)
    {
        _waveItemData = waveItemData;

        if (_costText != null)
        {
            _costText.text = _spinManager.GetReviveCost().ToString();
        }

        if (_itemIcon != null)
        {
            ItemData itemData = _itemManager.GetData(waveItemData.type);
            _itemIcon.sprite = itemData?.icon;
        }

        if (_reviveButton != null)
        {
            _reviveButton.interactable = _currencyManager.Get(CurrencyType.Coin) >= _spinManager.GetReviveCost();
        }
    }

    protected override PopupType GetPopupType()
    {
        return PopupType.DeathPopUp;
    }

    private void OnGiveUpClicked()
    {
        _signalBus.Fire(new DeathPopupDecisionSignal(DeathPopupDecision.GiveUp));
        Close();
    }

    private void OnReviveClicked()
    {
        if (!_currencyManager.Remove(CurrencyType.Coin, (int)_spinManager.GetReviveCost()))
        {
            return;
        }

        _signalBus.Fire(new DeathPopupDecisionSignal(DeathPopupDecision.Revive));
        Close();
    }
}
