using System;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Settings")]
public class ScriptableSettings : ScriptableObject
{
    [field: SerializeField] public Level[] Levels { get; private set; } = Array.Empty<Level>();
    [field: SerializeField,Scene] public int[] LevelScenes { get; private set; } = Array.Empty<int>();
    [field: SerializeField] public int DefaultStartIndex { get; private set; } = 0;

    public void SetStartIndex(int index)
    {
        if (index < 0) index = 0;
        if (index >= Levels.Length) index = Levels.Length - 1;
        DefaultStartIndex = index;
    }
    
    [field: SerializeField] public Sprite[] bottleShapesSprites { get; private set; }
    [field: SerializeField] public Sprite[] bottleContentSprites { get; private set; }

    public static ScriptableSettings GlobalSettings { get; private set; }

    public void SetAsGlobalSettings()
    {
        GlobalSettings = this;
        Debug.Log($"Global Settings set to {GlobalSettings}");
    }
}
