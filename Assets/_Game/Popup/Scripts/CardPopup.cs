using System;
using System.Reflection;
using Popup;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CardPopup : IPopup
{
    private const float OverlayAlpha = 0.72f;

    private ItemManager _itemManager;
    
    [Header("COMPONENTS")]
    [SerializeField]private Image _icon;
    [SerializeField]private TMP_Text _titleText;
    [SerializeField]private TMP_Text _amountText;
    [SerializeField]private RectTransform _particleSource;

    [Header("READONLY")]
    [ShowInInspector,ReadOnly]private SpinWaveItemData _waveItemData;
    [ShowInInspector,ReadOnly]private GainedItemView _targetView;
    [ShowInInspector,ReadOnly]private CurrencyParticleListener _particleListener;
    [ShowInInspector,ReadOnly]private Transform _previousAttractor;
    [ShowInInspector,ReadOnly]private bool _isClosingSequenceActive;
    
    private Action _onRewardSequenceFinished;

    [Inject]
    private void InjectItemManager(ItemManager itemManager)
    {
        _itemManager = itemManager;
    }

    public void Setup(SpinWaveItemData waveItemData, Action onRewardSequenceFinished)
    {
        _waveItemData = waveItemData;
        _targetView = _itemManager.GetCurrentGainedView();
        _onRewardSequenceFinished = onRewardSequenceFinished;
        _isClosingSequenceActive = false;
        RefreshContent();
    }

    protected override PopupType GetPopupType()
    {
        return PopupType.CardPopUp;
    }

    protected override void OnCloseStart()
    {
        if (_isClosingSequenceActive)
        {
            return;
        }

        _isClosingSequenceActive = true;
        Close(BeginRewardFlow);
    }

    protected override void OnHide()
    {
        RestoreParticleListener();
    }

    private void BeginRewardFlow()
    {
        if (_waveItemData == null || _targetView == null)
        {
            CompleteRewardFlow();
            return;
        }

        _particleListener = _itemManager.GetParticleListener(_waveItemData.type);

        if (_particleListener == null)
        {
            _targetView.Unlock(_waveItemData);
            CompleteRewardFlow();
            return;
        }

        _previousAttractor = _particleListener.Attractor;
        _particleListener.SetAttractor(_targetView.transform);
        _particleListener.OnLastParticleFinished += OnLastParticleFinished;

        float particleAmount = _waveItemData.multiple && _waveItemData.amount > 0 ? _waveItemData.amount : 1f;
        Transform particleSource = _particleSource != null ? _particleSource : transform;

        _signalBus.Fire(new CurrencyParticleSignal(
            _waveItemData.type,
            particleAmount,
            particleSource,
            CurrencyParticleListener.From.General,
            string.Empty));
    }

    private void OnLastParticleFinished(float _)
    {
        RestoreParticleListener();

        if (_targetView != null && _waveItemData != null)
        {
            _targetView.Unlock(_waveItemData);
        }

        CompleteRewardFlow();
    }

    private void CompleteRewardFlow()
    {
        Action callback = _onRewardSequenceFinished;
        _onRewardSequenceFinished = null;
        _targetView = null;
        _waveItemData = null;
        _isClosingSequenceActive = false;
        callback?.Invoke();
    }

    private void RefreshContent()
    {
        if (_waveItemData == null)
        {
            return;
        }

        ItemData itemData = _itemManager.GetData(_waveItemData.type);

        if (itemData?.icon != null && _icon != null)
        {
            _icon.sprite = itemData.icon;
        }

        if (_titleText != null)
        {
            _titleText.text = _waveItemData.type.ToString();
        }

        if (_amountText != null)
        {
            bool hasAmount = _waveItemData.multiple && _waveItemData.amount > 0;
            _amountText.gameObject.SetActive(hasAmount);
            _amountText.text = hasAmount ? $"x{_waveItemData.amount}" : string.Empty;
        }
    }

    private void RestoreParticleListener()
    {
        if (_particleListener == null)
        {
            return;
        }

        _particleListener.OnLastParticleFinished -= OnLastParticleFinished;
        _particleListener.SetAttractor(_previousAttractor);
        _particleListener = null;
        _previousAttractor = null;
    }
    
    
}
