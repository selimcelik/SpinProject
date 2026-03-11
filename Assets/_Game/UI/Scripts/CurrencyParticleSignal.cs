using UnityEngine;

public readonly struct CurrencyParticleSignal
{
    public readonly ItemType type;
    public readonly int amount;
    public readonly float rawAmount;
    public readonly Transform source;
    public readonly CurrencyParticleListener.From from;
    public readonly string itemId;
    public readonly float size;
    
    
    public CurrencyParticleSignal(ItemType itemType,float particleAmount, Transform source, CurrencyParticleListener.From from, string itemId, float size = 100)
    {
        type = itemType;
        amount = (int) particleAmount;
        rawAmount = amount;
        this.source = source;
        this.from = from;
        this.itemId = itemId;
        this.size = size;
    }
}
