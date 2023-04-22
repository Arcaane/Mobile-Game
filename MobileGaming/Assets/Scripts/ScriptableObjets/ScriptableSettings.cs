using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Settings")]
public class ScriptableSettings : ScriptableObject
{
    [field: SerializeField] public Level[] Levels { get; private set; } = Array.Empty<Level>();
    [field: SerializeField] public int DefaultStartIndex { get; private set; } = 0;
}
