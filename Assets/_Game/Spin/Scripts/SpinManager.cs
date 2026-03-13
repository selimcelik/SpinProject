using UnityEngine;
using Popup;
using Zenject;

public class SpinManager : IInitializable, System.IDisposable
{
    private const float TargetChanceTotal = 100f;

    private readonly SpinRepository _repository;
    private readonly SpinWaveData _waveData;
    private readonly PopupManager _popupManager;
    private readonly SignalBus _signalBus;
    
    private SpinController _controller;
    private SpinWaveItemData _pendingRewardData;
    private int _pendingResultIndex = -1;
    private bool _isRewardSequenceRunning;
    private bool _isWaitingDeathDecision;

    public Observable<int> CurrentWaveIndex { get; }
    public Observable<int> ClaimedRewardCount { get; }

    public SpinManager(SpinRepository repository, SpinWaveData waveData, PopupManager popupManager, SignalBus signalBus)
    {
        _repository = repository;
        _waveData = waveData;
        _popupManager = popupManager;
        _signalBus = signalBus;
        CurrentWaveIndex = new Observable<int>(0);
        ClaimedRewardCount = new Observable<int>(0);
    }

    public void Initialize()
    {
        _signalBus.Subscribe<DeathPopupDecisionSignal>(OnDeathPopupDecision);
    }

    public void Dispose()
    {
        _signalBus.TryUnsubscribe<DeathPopupDecisionSignal>(OnDeathPopupDecision);
    }

    public SpinData GetData(SpinType type)
    {
        return _repository.GetData(type);
    }

    public float GetReviveCost()
    {
        return _repository.reviveCost;
    }

    public SpinType GetCurrentSpinType()
    {
        SpinWave currentWave = GetCurrentWave();
        return currentWave == null ? SpinType.Undefined : currentWave.spinType;
    }

    public SpinWave GetCurrentWave()
    {
        return _waveData.GetWave(CurrentWaveIndex.Value);
    }

    public void RegisterSpinController(SpinController controller)
    {
        if (_controller == controller)
        {
            SendCurrentSpinType();
            return;
        }

        if (_controller != null)
        {
            _controller.SpinStarted -= OnSpinStarted;
            _controller.SpinCompleted -= OnSpinCompleted;
        }

        _controller = controller;
        _controller.SpinStarted += OnSpinStarted;
        _controller.SpinCompleted += OnSpinCompleted;
        SendCurrentSpinType();
        UpdateSpinAvailability(true);
    }

    public void UnregisterSpinController(SpinController controller)
    {
        if (_controller != controller)
        {
            return;
        }

        _controller.SpinStarted -= OnSpinStarted;
        _controller.SpinCompleted -= OnSpinCompleted;
        _controller = null;
    }

    public void AdvanceWave()
    {
        if (_waveData == null)
        {
            return;
        }
        
        SetCurrentWaveIndex(CurrentWaveIndex.Value + 1);
    }

    public void SetCurrentWaveIndex(int waveIndex)
    {
        if (waveIndex < 0)
        {
            return;
        }

        CurrentWaveIndex.Value = waveIndex;
        SendCurrentSpinType();
        UpdateSpinAvailability(false);
    }

    public void ResetProgress()
    {
        ClaimedRewardCount.Value = 0;
        _signalBus.Fire<SpinProgressResetSignal>();
        _isRewardSequenceRunning = false;
        _isWaitingDeathDecision = false;
        _pendingRewardData = null;
        _pendingResultIndex = -1;
        SetCurrentWaveIndex(0);
        UpdateSpinAvailability(true);
    }

    private void SendCurrentSpinType()
    {
        if (_controller == null)
        {
            return;
        }

        _controller.SetSpinType(GetCurrentSpinType());
    }

    private void OnSpinStarted()
    {
        if (_controller == null)
        {
            return;
        }

        _pendingResultIndex = SelectCurrentWaveResultIndex();

        if (_pendingResultIndex < 0)
        {
            return;
        }

        SpinWave currentWave = GetCurrentWave();
        _pendingRewardData = currentWave.items[_pendingResultIndex];
        _controller.SetSpinResultIndex(_pendingResultIndex);
    }

    private void OnSpinCompleted()
    {
        if (_pendingRewardData == null)
        {
            CompleteRewardSequence();
            return;
        }

        _isRewardSequenceRunning = true;

        if (_pendingRewardData.type == ItemType.Death)
        {
            _isWaitingDeathDecision = true;
            DeathPopup deathPopup = _popupManager.GetPopUp<DeathPopup>(PopupType.DeathPopUp);

            if (deathPopup == null)
            {
                ResolveDeathDecision(DeathPopupDecision.GiveUp);
                return;
            }

            deathPopup.Setup(_pendingRewardData);
            _popupManager.Open(deathPopup);
            return;
        }

        CardPopup popup = _popupManager.GetPopUp<CardPopup>(PopupType.CardPopUp);

        if (popup == null)
        {
            CompleteRewardSequence();
            return;
        }

        popup.Setup(_pendingRewardData, CompleteRewardSequence);
        _popupManager.Open(popup);
    }

    private int SelectCurrentWaveResultIndex()
    {
        SpinWave currentWave = GetCurrentWave();

        if (currentWave == null || currentWave.items == null || currentWave.items.Count == 0)
        {
            return -1;
        }

        float totalChance = 0f;

        for (int i = 0; i < currentWave.items.Count; i++)
        {
            float chance = Mathf.Max(0f, currentWave.items[i].chance);

            if (chance <= 0f)
            {
                continue;
            }

            totalChance += chance;
        }

        if (totalChance <= 0f)
        {
            return UnityEngine.Random.Range(0, currentWave.items.Count);
        }

        float normalizedTotalChance = Mathf.Min(totalChance, TargetChanceTotal);
        float roll = UnityEngine.Random.Range(0f, normalizedTotalChance);
        float cumulativeChance = 0f;

        for (int i = 0; i < currentWave.items.Count; i++)
        {
            float chance = Mathf.Max(0f, currentWave.items[i].chance);

            if (chance <= 0f)
            {
                continue;
            }

            cumulativeChance += chance;

            if (roll < cumulativeChance)
            {
                return i;
            }
        }

        return currentWave.items.Count - 1;
    }

    private void CompleteRewardSequence()
    {
        ClaimedRewardCount.Value += 1;
        FinalizeSpinFlow(true);
    }

    private void FinalizeSpinFlow(bool advanceWave)
    {
        _isRewardSequenceRunning = false;
        _isWaitingDeathDecision = false;
        _pendingRewardData = null;
        _pendingResultIndex = -1;

        if (advanceWave)
        {
            AdvanceWave();
        }

        UpdateSpinAvailability(false);
    }

    private void UpdateSpinAvailability(bool instant)
    {
        if (_controller == null)
        {
            return;
        }

        _controller.SetSpinAvailability(CanSpin(), instant);
    }

    private bool CanSpin()
    {
        if (_isRewardSequenceRunning || _isWaitingDeathDecision)
        {
            return false;
        }

        return GetCurrentWave() != null && ClaimedRewardCount.Value < 10;
    }

    private void OnDeathPopupDecision(DeathPopupDecisionSignal signal)
    {
        if (!_isWaitingDeathDecision)
        {
            return;
        }

        ResolveDeathDecision(signal.Decision);
    }

    private void ResolveDeathDecision(DeathPopupDecision decision)
    {
        switch (decision)
        {
            case DeathPopupDecision.Revive:
                FinalizeSpinFlow(false);
                break;
            case DeathPopupDecision.GiveUp:
                ResetProgress();
                break;
        }
    }
}
