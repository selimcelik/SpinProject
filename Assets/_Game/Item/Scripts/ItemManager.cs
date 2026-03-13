using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class ItemManager : IInitializable, IDisposable
{
    private readonly List<ItemView> _views = new List<ItemView>();
    private readonly List<GainedItemView> _gainedViews = new List<GainedItemView>();
    private readonly Dictionary<ItemType, CurrencyParticleListener> _particleListeners = new Dictionary<ItemType, CurrencyParticleListener>();
    
    private readonly SpinManager _spinManager;
    private readonly ItemRepository _repository;
    private readonly DiContainer _container;
    private readonly SignalBus _signalBus;

    private bool _isInitialized;

    public ItemManager(SpinManager spinManager, ItemRepository repository, DiContainer container, SignalBus signalBus)
    {
        _spinManager = spinManager;
        _repository = repository;
        _container = container;
        _signalBus = signalBus;
    }

    public void Initialize()
    {
        _isInitialized = true;
        BootstrapGainedViews();
        CacheParticleListeners();
        _spinManager.CurrentWaveIndex.OnChanged += OnCurrentWaveIndexChanged;
        _signalBus.Subscribe<SpinProgressResetSignal>(OnSpinProgressReset);
        RefreshViews();
        RefreshGainedViews();
    }

    public void Dispose()
    {
        _spinManager.CurrentWaveIndex.OnChanged -= OnCurrentWaveIndexChanged;
        _signalBus.TryUnsubscribe<SpinProgressResetSignal>(OnSpinProgressReset);
        ClearAllViews();
    }

    public ItemData GetData(ItemType type)
    {
        return _repository.GetData(type);
    }

    public void AddView(ItemView view)
    {
        if (_views.Contains(view))
        {
            return;
        }

        _views.Add(view);

        if (_isInitialized)
        {
            RefreshView(view, _views.Count - 1);
        }
    }

    public void RemoveView(ItemView view)
    {
        if (!_views.Contains(view))
        {
            return;
        }

        _views.Remove(view);
    }

    public void AddGainedView(GainedItemView view)
    {
        if (_gainedViews.Contains(view))
        {
            return;
        }

        _gainedViews.Add(view);
        view.OnUnlocked += OnGainedViewOnUnlocked;

        if (_isInitialized)
        {
            RefreshGainedViews();
        }
    }

    public void RemoveGainedView(GainedItemView view)
    {
        if (!_gainedViews.Contains(view))
        {
            return;
        }

        view.OnUnlocked -= OnGainedViewOnUnlocked;
        _gainedViews.Remove(view);
    }

    public GainedItemView GetCurrentGainedView()
    {
        return GetOrderedGainedViews().FirstOrDefault(view => !view.IsUnlocked);
    }

    public CurrencyParticleListener GetParticleListener(ItemType type)
    {
        if (_particleListeners.TryGetValue(type, out CurrencyParticleListener listener) && listener != null)
        {
            return listener;
        }

        CacheParticleListeners();
        _particleListeners.TryGetValue(type, out listener);
        return listener;
    }

    public int ClaimCoinRewards()
    {
        int totalCoinAmount = 0;

        foreach (GainedItemView view in _gainedViews)
        {
            if (view == null || !view.IsUnlocked || view.IsCurrencyClaimed || view.RewardData == null)
            {
                continue;
            }

            if (view.RewardData.type != ItemType.Gold && view.RewardData.type != ItemType.Cash)
            {
                continue;
            }

            totalCoinAmount += GetRewardAmount(view.RewardData);
            view.MarkCurrencyClaimed();
        }

        return totalCoinAmount;
    }

    private void RefreshViews()
    {
        SpinWave currentWave = _spinManager.GetCurrentWave();

        if (currentWave == null)
        {
            return;
        }

        List<ItemView> orderedViews = GetOrderedViews();
        int count = Math.Min(orderedViews.Count, currentWave.items.Count);

        for (int i = 0; i < count; i++)
        {
            RefreshView(orderedViews[i], i);
        }
    }

    private void RefreshView(ItemView view, int index)
    {
        SpinWave currentWave = _spinManager.GetCurrentWave();

        if (currentWave == null)
        {
            return;
        }

        if (index < 0 || index >= currentWave.items.Count)
        {
            return;
        }

        SpinWaveItemData itemData = currentWave.items[index];
        view.SetItemType(itemData);
    }

    private void OnCurrentWaveIndexChanged(int _)
    {
        if (!_isInitialized)
        {
            return;
        }

        RefreshViews();
    }

    private void BootstrapGainedViews()
    {
        RectTransform[] rectTransforms = UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (RectTransform rectTransform in rectTransforms)
        {
            if (rectTransform == null)
            {
                continue;
            }

            if (!rectTransform.name.StartsWith("Gained_Item_View", StringComparison.Ordinal))
            {
                continue;
            }

            if (!rectTransform.gameObject.scene.IsValid())
            {
                continue;
            }

            if (rectTransform.TryGetComponent(out GainedItemView _))
            {
                continue;
            }

            _container.InstantiateComponent<GainedItemView>(rectTransform.gameObject);
        }
    }

    private void CacheParticleListeners()
    {
        _particleListeners.Clear();
        CurrencyParticleListener[] listeners = UnityEngine.Object.FindObjectsByType<CurrencyParticleListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (CurrencyParticleListener listener in listeners)
        {
            if (listener == null || _particleListeners.ContainsKey(listener.Type))
            {
                continue;
            }

            _particleListeners.Add(listener.Type, listener);
        }
    }

    private List<ItemView> GetOrderedViews()
    {
        return _views
            .OrderBy(view => GetLogicalIndex(view.transform as RectTransform))
            .ToList();
    }

    private List<GainedItemView> GetOrderedGainedViews()
    {
        return _gainedViews
            .OrderBy(view => view.transform.GetSiblingIndex())
            .ToList();
    }

    private void RefreshGainedViews()
    {
        List<GainedItemView> orderedViews = GetOrderedGainedViews();
        bool currentAssigned = false;

        foreach (GainedItemView view in orderedViews)
        {
            if (view == null)
            {
                continue;
            }

            bool isCurrentLocked = !view.IsUnlocked && !currentAssigned;
            view.RefreshState(isCurrentLocked);

            if (isCurrentLocked)
            {
                currentAssigned = true;
            }
        }
    }

    private void OnGainedViewOnUnlocked(GainedItemView _)
    {
        RefreshGainedViews();
    }

    private void OnSpinProgressReset()
    {
        foreach (GainedItemView view in _gainedViews)
        {
            view?.ResetView();
        }

        RefreshGainedViews();
        RefreshViews();
    }

    private int GetLogicalIndex(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            return int.MaxValue;
        }

        Vector2 anchoredPosition = rectTransform.anchoredPosition;
        float angle = Mathf.Atan2(anchoredPosition.y, anchoredPosition.x) * Mathf.Rad2Deg;
        float clockwiseAngleFromTop = Mathf.Repeat(90f - angle, 360f);
        return Mathf.RoundToInt(clockwiseAngleFromTop / 45f) % 8;
    }

    private void ClearAllViews()
    {
        _views.Clear();
        _gainedViews.Clear();
        _particleListeners.Clear();
    }

    private static int GetRewardAmount(SpinWaveItemData waveItemData)
    {
        if (waveItemData == null)
        {
            return 0;
        }

        if (waveItemData.multiple && waveItemData.amount > 0)
        {
            return waveItemData.amount;
        }

        return 1;
    }
}
