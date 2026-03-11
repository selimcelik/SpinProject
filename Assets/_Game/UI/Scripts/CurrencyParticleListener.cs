using System;
using System.Collections.Generic;
using AssetKits.ParticleImage;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(ParticleImage))]
public class CurrencyParticleListener : MonoBehaviour
{
    public enum From { General = 0, Shop = 1 }

    public event Action<float> OnLastParticleFinished;

    [Header("VALUES")] 
    [SerializeField] private ItemType _type;
    [SerializeField] private From _from;
    [SerializeField] private Transform _attractor;
    [SerializeField] [Min(1)] private int _maxBurst = 20;

    [Header("READONLY")]
    [ShowInInspector] [ReadOnly] private int _pendingAmount;
    [ShowInInspector] [ReadOnly] private float _pendingRawAmount;
    [ShowInInspector] [ReadOnly] private int _pendingDamagePackets;
    
    private ParticleImage _particle;
    
    private SignalBus _signalBus;

    public ItemType Type => _type;
    public Transform Attractor => _attractor;

    [Inject]
    private void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
        
        _signalBus.Subscribe<CurrencyParticleSignal>(OnCollectParticle);
        TryGetComponent(out _particle);
    }

    private void Awake()
    {
        _particle.onLastParticleFinish.AddListener(OnLastParticleFinish);
        _particle.attractorTarget = _attractor;
    }

    private void OnDestroy()
    {
        _signalBus.TryUnsubscribe<CurrencyParticleSignal>(OnCollectParticle);
        
        _particle.onLastParticleFinish.RemoveListener(OnLastParticleFinish);

    }

    private void OnCollectParticle(CurrencyParticleSignal signal)
    {
        if (_from != signal.from)
        {
            return;
        }

        if (_type != signal.type)
        {
            return;
        }

        _pendingAmount += signal.amount;
        _pendingRawAmount += signal.rawAmount;
        _particle.emitterConstraintTransform = signal.source;
        _particle.startSize = new SeparatedMinMaxCurve(signal.size);

        if (_particle.isPlaying)
        {
            return;
        }

        int particleAmount = Mathf.Min(_pendingAmount, _maxBurst);
        
        _particle.SetBurst(0, 0, particleAmount);

        _particle.Play();
    }

    private void OnLastParticleFinish()
    {
        if (_pendingAmount > 0)
        {
            OnLastParticleFinished?.Invoke(_pendingRawAmount);
            
            _pendingAmount = 0;
            _pendingRawAmount = 0f;
        }
    }

    public void SetAttractor(Transform attractor)
    {
        _attractor = attractor;

        if (_particle != null)
        {
            _particle.attractorTarget = attractor;
        }
    }
}
