using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpinWaveData))]
public class SpinWaveDataEditor : Editor
{
    private const int MaxItemCount = 8;
    private const float TargetChanceTotal = 100f;

    private static readonly ItemType[] ValidItemTypes =
    {
        ItemType.AviatorGlasses,
        ItemType.BaseballCap,
        ItemType.ChestBigNolight,
        ItemType.ChestBronzeNolight,
        ItemType.ChestGoldNolight,
        ItemType.ChestSilverNolight,
        ItemType.ChestSmallNolight,
        ItemType.ChestStandardNolight,
        ItemType.ChestSuperNolight,
        ItemType.Gold,
        ItemType.Cash,
        ItemType.PumpkinHelmet,
        ItemType.BayonetEaster,
        ItemType.BayonetSummer,
        ItemType.GrenadeM26,
        ItemType.GrenadeM67,
        ItemType.HealthShotNeurostim,
        ItemType.HealthShotRegenerator,
        ItemType.Molotov,
        ItemType.ShotgunLv1,
        ItemType.Mle,
        ItemType.Rifle,
        ItemType.Smg,
        ItemType.Sniper,
        ItemType.ArmorPoints,
        ItemType.KnifePoints,
        ItemType.PistolPoints,
        ItemType.RiflePoints,
        ItemType.SmgPoints,
        ItemType.SniperPoints,
        ItemType.ShotgunPoints,
        ItemType.SubmachinePoints,
        ItemType.VestPoints,
        ItemType.ShotgunLv2,
        ItemType.Death
    };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty wavesProperty = serializedObject.FindProperty("_waves");
        EditorGUILayout.PropertyField(wavesProperty, true);

        bool isChanged = SyncWaveIndexes(wavesProperty);
        isChanged |= FixWaveItems(wavesProperty);

        DrawChanceSummaries(wavesProperty);

        if (isChanged)
        {
            EditorGUILayout.HelpBox("Wave verileri otomatik duzenlendi. Her wave tam 8 item, Undefined disi degerler, toplam 100 chance ve en az 1 Death item icerir.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private bool SyncWaveIndexes(SerializedProperty wavesProperty)
    {
        bool isChanged = false;

        for (int i = 0; i < wavesProperty.arraySize; i++)
        {
            SerializedProperty waveProperty = wavesProperty.GetArrayElementAtIndex(i);
            SerializedProperty waveIndexProperty = waveProperty.FindPropertyRelative("waveIndex");

            if (waveIndexProperty.intValue == i)
            {
                continue;
            }

            waveIndexProperty.intValue = i;
            isChanged = true;
        }

        return isChanged;
    }

    private bool FixWaveItems(SerializedProperty wavesProperty)
    {
        bool isChanged = false;

        for (int i = 0; i < wavesProperty.arraySize; i++)
        {
            SerializedProperty waveProperty = wavesProperty.GetArrayElementAtIndex(i);
            SerializedProperty itemsProperty = waveProperty.FindPropertyRelative("items");

            while (itemsProperty.arraySize > MaxItemCount)
            {
                itemsProperty.DeleteArrayElementAtIndex(itemsProperty.arraySize - 1);
                isChanged = true;
            }

            while (itemsProperty.arraySize < MaxItemCount)
            {
                int newIndex = itemsProperty.arraySize;
                itemsProperty.InsertArrayElementAtIndex(newIndex);
                SerializedProperty itemProperty = itemsProperty.GetArrayElementAtIndex(newIndex);
                SetDefaultItemData(itemProperty, GetRandomItemType());
                isChanged = true;
            }

            for (int itemIndex = 0; itemIndex < itemsProperty.arraySize; itemIndex++)
            {
                SerializedProperty itemProperty = itemsProperty.GetArrayElementAtIndex(itemIndex);
                SerializedProperty itemTypeProperty = itemProperty.FindPropertyRelative("type");
                SerializedProperty chanceProperty = itemProperty.FindPropertyRelative("chance");
                SerializedProperty multipleProperty = itemProperty.FindPropertyRelative("multiple");
                SerializedProperty amountProperty = itemProperty.FindPropertyRelative("amount");

                if (itemTypeProperty.enumValueIndex == (int)ItemType.Undefined)
                {
                    itemTypeProperty.enumValueIndex = (int)GetRandomItemType();
                    isChanged = true;
                }

                if (chanceProperty.floatValue < 0f)
                {
                    chanceProperty.floatValue = 0f;
                    isChanged = true;
                }

                if (!multipleProperty.boolValue && amountProperty.intValue != 0)
                {
                    amountProperty.intValue = 0;
                    isChanged = true;
                }

                if (multipleProperty.boolValue && amountProperty.intValue <= 0)
                {
                    amountProperty.intValue = 1;
                    isChanged = true;
                }
            }

            if (!HasDeathItem(itemsProperty))
            {
                int deathIndex = Random.Range(0, itemsProperty.arraySize);
                SerializedProperty deathItemProperty = itemsProperty.GetArrayElementAtIndex(deathIndex);
                deathItemProperty.FindPropertyRelative("type").enumValueIndex = (int)ItemType.Death;
                isChanged = true;
            }

            isChanged |= BalanceChanceTotal(itemsProperty);
        }

        return isChanged;
    }

    private bool BalanceChanceTotal(SerializedProperty itemsProperty)
    {
        bool isChanged = false;
        float totalChance = 0f;

        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            SerializedProperty chanceProperty = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("chance");
            chanceProperty.floatValue = Mathf.Max(0f, chanceProperty.floatValue);
            totalChance += chanceProperty.floatValue;
        }

        if (Mathf.Approximately(totalChance, TargetChanceTotal))
        {
            return isChanged;
        }

        if (totalChance > TargetChanceTotal)
        {
            float overflow = totalChance - TargetChanceTotal;

            for (int i = itemsProperty.arraySize - 1; i >= 0 && overflow > 0f; i--)
            {
                SerializedProperty chanceProperty = itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("chance");
                float reduction = Mathf.Min(chanceProperty.floatValue, overflow);
                chanceProperty.floatValue -= reduction;
                overflow -= reduction;
                isChanged |= reduction > 0f;
            }

            return isChanged;
        }

        float missingChance = TargetChanceTotal - totalChance;
        SerializedProperty lastChanceProperty = itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1).FindPropertyRelative("chance");
        lastChanceProperty.floatValue += missingChance;
        return true;
    }

    private void DrawChanceSummaries(SerializedProperty wavesProperty)
    {
        EditorGUILayout.Space(8f);

        for (int i = 0; i < wavesProperty.arraySize; i++)
        {
            SerializedProperty waveProperty = wavesProperty.GetArrayElementAtIndex(i);
            SerializedProperty itemsProperty = waveProperty.FindPropertyRelative("items");
            float totalChance = GetChanceTotal(itemsProperty);

            EditorGUILayout.LabelField($"Wave {i} Chance Toplami", $"{totalChance:0.##} / {TargetChanceTotal:0}");
        }
    }

    private float GetChanceTotal(SerializedProperty itemsProperty)
    {
        float totalChance = 0f;

        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            totalChance += itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("chance").floatValue;
        }

        return totalChance;
    }

    private bool HasDeathItem(SerializedProperty itemsProperty)
    {
        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            SerializedProperty itemProperty = itemsProperty.GetArrayElementAtIndex(i);
            SerializedProperty itemTypeProperty = itemProperty.FindPropertyRelative("type");

            if (itemTypeProperty.enumValueIndex == (int)ItemType.Death)
            {
                return true;
            }
        }

        return false;
    }

    private void SetDefaultItemData(SerializedProperty itemProperty, ItemType itemType)
    {
        itemProperty.FindPropertyRelative("type").enumValueIndex = (int)itemType;
        itemProperty.FindPropertyRelative("chance").floatValue = 0f;
        itemProperty.FindPropertyRelative("multiple").boolValue = false;
        itemProperty.FindPropertyRelative("amount").intValue = 0;
    }

    private ItemType GetRandomItemType()
    {
        int randomIndex = Random.Range(0, ValidItemTypes.Length);
        return ValidItemTypes[randomIndex];
    }
}
