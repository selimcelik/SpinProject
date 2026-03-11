using System;
using System.Reflection;
using Coffee.UIEffects;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SpinController : MonoBehaviour
{
    public event Action SpinStarted;
    public event Action SpinCompleted;

    [Header("COMPONENTS")]
    [SerializeField] private Image _indicator;
    [SerializeField] private Image _base;
    [SerializeField] private RectTransform _itemViewsRoot;
    [SerializeField] private Button _spinButton; 
    [SerializeField] private RectTransform _baseRect;
    [SerializeField] private RectTransform _indicatorRect;
    [SerializeField] private RectTransform _spinButtonRect;
    [SerializeField] private UIEffect _spinButtonEffect;

    [Header("SPIN")]
    [SerializeField] private Vector2 _spinDurationRange = new Vector2(4f, 6f);
    [SerializeField] private Vector2Int _fullRotationRange = new Vector2Int(5, 8);
    [SerializeField] private float _slotAngle = 45f;
    [SerializeField] private float _stopAngleOffset;

    [Header("INDICATOR")]
    [SerializeField] private float _indicatorRestAngle;
    [SerializeField] private float _indicatorKickAngle = 22f;
    [SerializeField] private float _indicatorKickDuration = 0.04f;
    [SerializeField] private float _indicatorReturnDuration = 0.12f;
    [SerializeField] private float _indicatorMinSpeedForTick = 90f;

    [Header("BUTTON")]
    [SerializeField] private float _buttonPressScale = 1.08f;
    [SerializeField] private float _buttonPressDuration = 0.08f;
    [SerializeField] private float _buttonReleaseDuration = 0.12f;
    [SerializeField] private float _buttonDisabledGrayscale = 1f;
    [SerializeField] private float _buttonEffectTweenDuration = 0.18f;

    [Header("READ-ONLY")]
    [ShowInInspector, ReadOnly] private SpinType _type;
    [ShowInInspector, ReadOnly] private SpinData _spinData;
    [ShowInInspector, ReadOnly] private bool _isSpinning;
    [ShowInInspector, ReadOnly] private int _resultIndex = -1;
    [ShowInInspector, ReadOnly]private float _currentWheelAngle;
    [ShowInInspector, ReadOnly]private float _lastIndicatorTriggerAngle;
    [ShowInInspector, ReadOnly]private float _buttonToneIntensity;

    private SpinManager _manager;
   
    private Tween _spinTween;
    private Tween _buttonStateTween;
    private Sequence _buttonClickSequence;
    private Sequence _indicatorSequence;
    private Vector3 _buttonDefaultScale;

    [Inject]
    private void Construct(SpinManager manager)
    {
        _manager = manager;
    }

    private void Awake()
    {
        _buttonDefaultScale = _spinButtonRect != null ? _spinButtonRect.localScale : Vector3.one;
        _currentWheelAngle = _baseRect != null ? _baseRect.localEulerAngles.z : 0f;
    }

    private void OnEnable()
    {
        _manager.RegisterSpinController(this);

        if (_spinButton != null)
        {
            _spinButton.onClick.AddListener(OnSpinButtonClicked);
        }

        ResetIndicatorInstant();
        ApplyWheelAngle(_currentWheelAngle);
    }

    private void OnDisable()
    {
        if (_spinButton != null)
        {
            _spinButton.onClick.RemoveListener(OnSpinButtonClicked);
        }

        _manager.UnregisterSpinController(this);
        KillTweens();
    }

    private void OnDestroy()
    {
        KillTweens();
    }

    private void OnValidate()
    {
        if (_spinDurationRange.x > _spinDurationRange.y)
        {
            _spinDurationRange.y = _spinDurationRange.x;
        }

        if (_fullRotationRange.x > _fullRotationRange.y)
        {
            _fullRotationRange.y = _fullRotationRange.x;
        }

        if (_slotAngle <= 0f)
        {
            _slotAngle = 45f;
        }
    }

    private void OnSpinButtonClicked()
    {
        if (_isSpinning)
        {
            return;
        }

        PlayButtonClickAnimation();
        StartSpin();
    }

    private void StartSpin()
    {
        SpinWave currentWave = _manager.GetCurrentWave();

        if (currentWave == null || currentWave.items == null || currentWave.items.Count == 0)
        {
            return;
        }

        _resultIndex = -1;
        SpinStarted?.Invoke();

        if (_resultIndex < 0)
        {
            return;
        }

        _isSpinning = true;
        _lastIndicatorTriggerAngle = 0f;
        SetSpinButtonState(false, false);

        int extraRotations = UnityEngine.Random.Range(_fullRotationRange.x, _fullRotationRange.y + 1);
        float duration = UnityEngine.Random.Range(_spinDurationRange.x, _spinDurationRange.y);
        float targetAngle = GetTargetAngle(_resultIndex, extraRotations);

        _spinTween?.Kill();
        _spinTween = DOTween.To(() => _currentWheelAngle, UpdateWheelAngle, targetAngle, duration)
            .SetEase(Ease.OutQuart)
            .OnComplete(CompleteSpin)
            .SetLink(gameObject);
    }

    private void CompleteSpin()
    {
        _isSpinning = false;
        ResetIndicatorInstant();
        SpinCompleted?.Invoke();
    }

    private void UpdateWheelAngle(float angle)
    {
        float angleDelta = angle - _currentWheelAngle;
        _currentWheelAngle = angle;
        ApplyWheelAngle(angle);
        UpdateIndicatorTick(angleDelta);
    }

    public void SetSpinResultIndex(int resultIndex)
    {
        _resultIndex = resultIndex;
    }

    public void SetSpinType(SpinType type)
    {
        _type = type;
        _spinData = _manager.GetData(_type);
        SetSpinLook();
    }

    public void SetSpinAvailability(bool isAvailable, bool instant = false)
    {
        if (_isSpinning && isAvailable)
        {
            return;
        }

        SetSpinButtonState(isAvailable, instant);
    }

    private void SetSpinLook()
    {
        if (_spinData == null)
        {
            return;
        }

        _base.sprite = _spinData.baseIcon;
        _indicator.sprite = _spinData.indicatorIcon;
    }

    private void ApplyWheelAngle(float angle)
    {
        if (_baseRect != null)
        {
            _baseRect.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (_itemViewsRoot != null)
        {
            _itemViewsRoot.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private float GetTargetAngle(int resultIndex, int extraRotations)
    {
        float currentNormalized = NormalizeAngle(_currentWheelAngle);
        float desiredNormalized = NormalizeAngle(GetDesiredStopAngle(resultIndex));
        float clockwiseDelta = Mathf.Repeat(currentNormalized - desiredNormalized, 360f);
        return _currentWheelAngle - clockwiseDelta - (extraRotations * 360f);
    }

    private float GetDesiredStopAngle(int resultIndex)
    {
        RectTransform resultView = GetResultView(resultIndex);

        if (resultView == null)
        {
            return _stopAngleOffset - (resultIndex * _slotAngle);
        }

        Vector2 position = resultView.anchoredPosition;
        float slotAngle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
        float indicatorAngle = 90f + _stopAngleOffset;
        return indicatorAngle - slotAngle;
    }

    private RectTransform GetResultView(int resultIndex)
    {
        if (_itemViewsRoot == null || resultIndex < 0)
        {
            return null;
        }

        for (int i = 0; i < _itemViewsRoot.childCount; i++)
        {
            RectTransform child = _itemViewsRoot.GetChild(i) as RectTransform;

            if (GetLogicalIndex(child) == resultIndex)
            {
                return child;
            }
        }

        return null;
    }

    private int GetLogicalIndex(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            return -1;
        }

        Vector2 anchoredPosition = rectTransform.anchoredPosition;
        float angle = Mathf.Atan2(anchoredPosition.y, anchoredPosition.x) * Mathf.Rad2Deg;
        float clockwiseAngleFromTop = Mathf.Repeat(90f - angle, 360f);
        return Mathf.RoundToInt(clockwiseAngleFromTop / 45f) % 8;
    }

    private void UpdateIndicatorTick(float angleDelta)
    {
        if (_indicatorRect == null || Mathf.Approximately(angleDelta, 0f))
        {
            return;
        }

        float speed = Mathf.Abs(angleDelta) / Mathf.Max(Time.deltaTime, 0.0001f);

        if (speed < _indicatorMinSpeedForTick)
        {
            return;
        }

        _lastIndicatorTriggerAngle += Mathf.Abs(angleDelta);

        while (_lastIndicatorTriggerAngle >= _slotAngle)
        {
            _lastIndicatorTriggerAngle -= _slotAngle;
            PlayIndicatorTick(Mathf.Sign(angleDelta), speed);
        }
    }

    private void PlayIndicatorTick(float direction, float speed)
    {
        if (_indicatorRect == null)
        {
            return;
        }

        float speedRatio = Mathf.Clamp01(speed / 1440f);
        float kickDuration = Mathf.Lerp(_indicatorKickDuration * 1.3f, _indicatorKickDuration, speedRatio);
        float returnDuration = Mathf.Lerp(_indicatorReturnDuration * 1.2f, _indicatorReturnDuration, speedRatio);
        float kickAngle = _indicatorRestAngle + (_indicatorKickAngle * -direction);

        _indicatorSequence?.Kill();
        _indicatorSequence = DOTween.Sequence()
            .Append(_indicatorRect.DOLocalRotate(new Vector3(0f, 0f, kickAngle), kickDuration).SetEase(Ease.OutQuad))
            .Append(_indicatorRect.DOLocalRotate(new Vector3(0f, 0f, _indicatorRestAngle), returnDuration).SetEase(Ease.OutBack))
            .SetLink(gameObject);
    }

    private void ResetIndicatorInstant()
    {
        if (_indicatorRect == null)
        {
            return;
        }

        _indicatorSequence?.Kill();
        _indicatorRect.localRotation = Quaternion.Euler(0f, 0f, _indicatorRestAngle);
    }

    private void PlayButtonClickAnimation()
    {
        if (_spinButtonRect == null)
        {
            return;
        }

        _buttonClickSequence?.Kill();
        _spinButtonRect.localScale = _buttonDefaultScale;
        _buttonClickSequence = DOTween.Sequence()
            .Append(_spinButtonRect.DOScale(_buttonDefaultScale * _buttonPressScale, _buttonPressDuration).SetEase(Ease.OutQuad))
            .Append(_spinButtonRect.DOScale(_buttonDefaultScale, _buttonReleaseDuration).SetEase(Ease.OutBack))
            .SetLink(gameObject);
    }

    private void SetSpinButtonState(bool isAvailable, bool instant)
    {
        if (_spinButton != null)
        {
            _spinButton.interactable = isAvailable;
        }

        float targetToneIntensity = isAvailable ? 0f : _buttonDisabledGrayscale;

        _buttonStateTween?.Kill();

        if (instant)
        {
            ApplySpinButtonToneIntensity(targetToneIntensity);
            return;
        }

        _buttonStateTween = DOTween.To(
                () => _buttonToneIntensity,
                ApplySpinButtonToneIntensity,
                targetToneIntensity,
                _buttonEffectTweenDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void ApplySpinButtonToneIntensity(float value)
    {
        _buttonToneIntensity = value;

        if (_spinButtonEffect == null)
        {
            return;
        }

        TrySetEffectValue(_spinButtonEffect, "toneIntensity", value);
        TrySetEffectValue(_spinButtonEffect, "ToneIntensity", value);
        TrySetEffectValue(_spinButtonEffect, "m_ToneIntensity", value);
        TryInvokeEffectMethod(_spinButtonEffect, "SetVerticesDirty");
        TryInvokeEffectMethod(_spinButtonEffect, "SetMaterialDirty");
    }

    private static bool TrySetEffectValue(Component effect, string memberName, float value)
    {
        if (effect == null)
        {
            return false;
        }

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        PropertyInfo property = effect.GetType().GetProperty(memberName, flags);

        if (property != null && property.CanWrite && property.PropertyType == typeof(float))
        {
            property.SetValue(effect, value);
            return true;
        }

        FieldInfo field = effect.GetType().GetField(memberName, flags);

        if (field != null && field.FieldType == typeof(float))
        {
            field.SetValue(effect, value);
            return true;
        }

        return false;
    }

    private static void TryInvokeEffectMethod(Component effect, string methodName)
    {
        if (effect == null)
        {
            return;
        }

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        MethodInfo method = effect.GetType().GetMethod(methodName, flags);
        method?.Invoke(effect, null);
    }

    private void KillTweens()
    {
        _spinTween?.Kill();
        _buttonStateTween?.Kill();
        _buttonClickSequence?.Kill();
        _indicatorSequence?.Kill();
    }
    private static float NormalizeAngle(float angle)
    {
        return Mathf.Repeat(angle, 360f);
    }
}
