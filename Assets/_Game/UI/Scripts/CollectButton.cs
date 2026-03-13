using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CollectButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private RectTransform _buttonRect;
    [SerializeField] private float _pulseScale = 1.06f;
    [SerializeField] private float _pulseDuration = 0.45f;
    [SerializeField] private float _buttonPressScale = 1.08f;
    [SerializeField] private float _buttonPressDuration = 0.08f;
    [SerializeField] private float _buttonReleaseDuration = 0.12f;

    private SpinManager _spinManager;
    private ItemManager _itemManager;
    private CurrencyManager _currencyManager;
    private Vector3 _defaultScale;
    private Tween _pulseTween;
    private Sequence _clickSequence;

    [Inject]
    private void Construct(SpinManager spinManager, ItemManager itemManager, CurrencyManager currencyManager)
    {
        _spinManager = spinManager;
        _itemManager = itemManager;
        _currencyManager = currencyManager;
    }

    private void Awake()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_buttonRect == null)
        {
            _buttonRect = transform as RectTransform;
        }

        _defaultScale = _buttonRect != null ? _buttonRect.localScale : Vector3.one;
    }

    private void OnEnable()
    {
        _spinManager.ClaimedRewardCount.OnChanged += OnClaimedRewardCountChanged;
        
        if (_button != null)
        {
            _button.onClick.AddListener(OnCollectClicked);
        }
        
        RefreshState(_spinManager.ClaimedRewardCount.Value);
    }

    private void OnDisable()
    {
        _spinManager.ClaimedRewardCount.OnChanged -= OnClaimedRewardCountChanged;

        if (_button != null)
        {
            _button.onClick.RemoveListener(OnCollectClicked);
        }
        
        StopPulse();
    }

    private void OnClaimedRewardCountChanged(int claimedRewardCount)
    {
        RefreshState(claimedRewardCount);
    }

    private void RefreshState(int claimedRewardCount)
    {
        bool canCollect = claimedRewardCount > 0;

        if (_button != null)
        {
            _button.interactable = canCollect;
        }

        if (canCollect)
        {
            StartPulse();
            return;
        }

        StopPulse();
    }

    private void StartPulse()
    {
        if (_buttonRect == null || _pulseTween != null && _pulseTween.IsActive())
        {
            return;
        }

        _buttonRect.localScale = _defaultScale;
        _pulseTween = _buttonRect.DOScale(_defaultScale * _pulseScale, _pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    private void StopPulse()
    {
        _pulseTween?.Kill();
        _pulseTween = null;

        if (_buttonRect != null)
        {
            _buttonRect.localScale = _defaultScale;
        }
    }

    private void OnCollectClicked()
    {
        PlayClickAnimation();

        int coinAmount = _itemManager.ClaimCoinRewards();

        if (coinAmount > 0)
        {
            _currencyManager.Add(CurrencyType.Coin, coinAmount);
        }

        _spinManager.ResetProgress();
    }

    private void PlayClickAnimation()
    {
        if (_buttonRect == null)
        {
            return;
        }

        _clickSequence?.Kill();
        _buttonRect.localScale = _defaultScale;
        _clickSequence = DOTween.Sequence()
            .Append(_buttonRect.DOScale(_defaultScale * _buttonPressScale, _buttonPressDuration).SetEase(Ease.OutQuad))
            .Append(_buttonRect.DOScale(_defaultScale, _buttonReleaseDuration).SetEase(Ease.OutBack))
            .SetLink(gameObject);
    }
}
