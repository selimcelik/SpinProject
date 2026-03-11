using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GainedItemView : MonoBehaviour
{
    public event Action<GainedItemView> OnUnlocked;

    public bool IsUnlocked => _isUnlocked;

    [Header("COMPONENTS")]
    [SerializeField]private GameObject _locked;
    [SerializeField]private GameObject _lockedBackground;
    [SerializeField]private GameObject _lockedCurrentBackground;
    [SerializeField]private GameObject _unlocked;
    [SerializeField]private Image _icon;
    [SerializeField]private TMP_Text _amountText;

    [Header("READONLY")]
    [ShowInInspector,ReadOnly] private bool _isUnlocked;
    
    private ItemManager _itemManager;

    [Inject]
    private void Construct(ItemManager itemManager)
    {
        _itemManager = itemManager;
    }

    private void Awake()
    {
        RefreshState(false);
    }

    private void OnEnable()
    {
        _itemManager?.AddGainedView(this);
    }

    private void OnDisable()
    {
        _itemManager?.RemoveGainedView(this);
    }

    public void RefreshState(bool isCurrentLocked)
    {

        if (_isUnlocked)
        {
            SetActiveState(false, false, true);
            return;
        }

        SetActiveState(true, !isCurrentLocked, false);
        SetCurrentBackgroundState(isCurrentLocked);
    }

    public void Unlock(SpinWaveItemData waveItemData)
    {
        if (waveItemData == null)
        {
            return;
        }

        ItemData itemData = _itemManager.GetData(waveItemData.type);

        if (itemData?.icon != null && _icon != null)
        {
            _icon.sprite = itemData.icon;
        }

        if (_amountText != null)
        {
            bool hasAmount = waveItemData.multiple && waveItemData.amount > 0;
            _amountText.gameObject.SetActive(hasAmount);
            _amountText.text = hasAmount ? $"x{waveItemData.amount}" : string.Empty;
        }

        _isUnlocked = true;
        SetActiveState(false, false, true);
        OnUnlocked?.Invoke(this);
    }

    private void SetActiveState(bool lockedActive, bool lockedBackgroundActive, bool unlockedActive)
    {
        if (_locked != null)
        {
            _locked.SetActive(lockedActive);
        }

        if (_unlocked != null)
        {
            _unlocked.SetActive(unlockedActive);
        }

        if (_lockedBackground != null)
        {
            _lockedBackground.SetActive(lockedActive && lockedBackgroundActive);
        }
    }

    private void SetCurrentBackgroundState(bool isCurrentLocked)
    {
        if (_lockedCurrentBackground != null)
        {
            _lockedCurrentBackground.SetActive(isCurrentLocked);
        }

        if (_lockedBackground != null)
        {
            _lockedBackground.SetActive(!isCurrentLocked);
        }
    }
}
