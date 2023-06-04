using System;
using UnityEngine;

[Serializable,CreateAssetMenu(menuName = "new Client")]
public class ScriptableClient : ScriptableObject
{
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public float BaseTimer { get; private set; } = 45f;
    [field: SerializeField] public float IncrementalTimer { get; private set; } = 15f;
    [field: SerializeField] public float TimerDecayPerSecond { get; private set; } = 1f;
    [field: SerializeField] public int BaseReward { get; private set; } = 10;
    [field: SerializeField] public int IncrementalReward { get; private set; } = 5;
    [field: SerializeField] public float BrewtifulPercent { get; private set; } = 0.8f;
    [field: SerializeField] public float GoodPercent { get; private set; } = 0.6f;

    [field: SerializeField] public ClientLook clientType;
}