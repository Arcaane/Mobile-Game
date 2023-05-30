using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Settings")]
public class ScriptableSettings : ScriptableObject
{
    [field:Header("Levels")]
    [field: SerializeField,Scene] public int[] LevelScenes { get; private set; } = Array.Empty<int>();
    [field: SerializeField] public int DefaultStartIndex { get; private set; } = 0;

    public void SetStartIndex(int index)
    {
        if (index < 0) index = 0;
        if (index >= LevelScenes.Length) index = LevelScenes.Length - 1;
        DefaultStartIndex = index;
    }
    
    [field:Header("Sprites")]
    [field: SerializeField] public Sprite[] bottleShapesSprites { get; private set; }
    [field: SerializeField] public Sprite[] bottleContentSprites { get; private set; }
    
    [field:Header("Items")]
    [field: SerializeField] public ScriptableItemDatabase itemDB { get; private set; }
    [field: SerializeField] private List<ScriptableItemEffect> equippedItemEffects;
    public static List<ScriptableItemEffect> EquippedItemEffects => GlobalSettings.equippedItemEffects;
    public static ScriptableSettings GlobalSettings { get; private set; }

    public void SetAsGlobalSettings()
    {
        GlobalSettings = this;
        Debug.Log($"Global Settings set to {GlobalSettings}");
    }

    public static void EquipItems(List<CollectionItem> items)
    {
        GlobalSettings.equippedItemEffects.Clear();
        foreach (var item in items)
        {
            GlobalSettings.equippedItemEffects.AddRange(item.ScriptableItemEffects);
        }
    }
}
