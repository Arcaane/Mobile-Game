using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Settings")]
public class ScriptableSettings : ScriptableObject
{
    [field: SerializeField] public Level[] Levels { get; private set; } = Array.Empty<Level>();
    [field: SerializeField] public int DefaultStartIndex { get; private set; } = 0;

    public void SetStartIndex(int index)
    {
        if (index < 0) index = 0;
        if (index >= Levels.Length) index = Levels.Length - 1;
        DefaultStartIndex = index;
    }
}
