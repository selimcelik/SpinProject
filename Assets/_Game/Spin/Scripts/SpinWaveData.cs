using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Spin Wave Data", menuName = "Spin/Spin Wave Data")]
public class SpinWaveData : ScriptableObject
{
    [SerializeField, TableList] private List<SpinWave> _waves = new List<SpinWave>();

    public List<SpinWave> GetWaves()
    {
        return _waves;
    }

    public SpinWave GetWave(int index)
    {
        if (_waves == null || _waves.Count == 0 || index < 0)
        {
            return null;
        }

        int wrappedIndex = index % _waves.Count;
        return _waves[wrappedIndex];
    }
}

[Serializable]
public class SpinWave
{
    [ReadOnly] public int waveIndex;
    public SpinType spinType;
    [ListDrawerSettings(Expanded = true)] public List<SpinWaveItemData> items = new List<SpinWaveItemData>(8);

    public bool HasDeathItem()
    {
        return items.Any(item => item.type == ItemType.Death);
    }

    public bool IsValid()
    {
        return items.Count == 8 && HasDeathItem();
    }
}

[Serializable]
public class SpinWaveItemData
{
    public ItemType type;
    public float chance = 1f;
    public bool multiple;
    [ShowIf("multiple")] public int amount;
}
