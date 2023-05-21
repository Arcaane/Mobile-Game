using System;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Settings")]
public class ScriptableSettings : ScriptableObject
{
    [field:Header("Levels")]
    [field: SerializeField] public Level[] Levels { get; private set; } = Array.Empty<Level>();
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
    [field: SerializeField] private ScriptableItem equippedItem;
    public static ScriptableItem EquippedItem => GlobalSettings.equippedItem;
    public static ScriptableSettings GlobalSettings { get; private set; }

    public void SetAsGlobalSettings()
    {
        GlobalSettings = this;
        Debug.Log($"Global Settings set to {GlobalSettings}");
    }

    public static void EquipItem(ScriptableItem item)
    {
        GlobalSettings.equippedItem = item;
    }
}
