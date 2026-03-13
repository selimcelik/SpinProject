using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Spin Repository", menuName = "Spin/Spin Repository")]
public class SpinRepository : ScriptableObject
{
    public float reviveCost;
    [SerializeField, TableList] private List<SpinData> _datas = new List<SpinData>();

    public SpinData GetData(SpinType type)
    {
        return _datas.FirstOrDefault(data => data.type == type);
    }
}

[Serializable]
public class SpinData
{
    public SpinType type;
    [PreviewField]public Sprite baseIcon;
    [PreviewField]public Sprite indicatorIcon;
}

public enum SpinType
{
    Undefined = 0,
    BronzeSpin=1,
    SilverSpin=2,
    GoldSpin=3
}
