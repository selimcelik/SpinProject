using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Repository", menuName = "Item/Item Repository")]
public class ItemRepository : ScriptableObject
{
    [SerializeField,TableList] private List<ItemData> _datas = new List<ItemData>();

    public ItemData GetData(ItemType type)
    {
        return _datas.FirstOrDefault(data => data.type == type);
    }
}

public enum ItemType
{
    Undefined = 0,
    AviatorGlasses=1,
    BaseballCap=2,
    ChestBigNolight=3,
    ChestBronzeNolight=4,
    ChestGoldNolight=5,
    ChestSilverNolight=6,
    ChestSmallNolight=7,
    ChestStandardNolight=8,
    ChestSuperNolight=9,
    Gold=10,
    Cash=11,
    PumpkinHelmet=12,
    BayonetEaster=13,
    BayonetSummer=14,
    GrenadeM26=15,
    GrenadeM67=16,
    HealthShotNeurostim=17,
    HealthShotRegenerator=18,
    Molotov=19,
    ShotgunLv1=20,
    Mle=21,
    Rifle=22,
    Smg=23,
    Sniper=24,
    ArmorPoints=25,
    KnifePoints=26,
    PistolPoints=27,
    RiflePoints=28,
    SmgPoints=29,
    SniperPoints=30,
    ShotgunPoints=31,
    SubmachinePoints=32,
    VestPoints=33,
    ShotgunLv2=34,
    Death = 35,
}

[Serializable]
public class ItemData
{
    public ItemType type;
    [PreviewField]public Sprite icon;
}
