using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Increase Level Duration")]
public class ScriptableItemIncreaseTimer : ScriptableItemEffect
{
    [field: SerializeField,Tooltip("Percent of Base Duration")] public float TimerIncreaseAmount { get; private set; } = 10f;
    protected override void Effect(LevelService levelService)
    {
        levelService.IncreaseLevelDuration(levelService.CurrentLevel.levelDuration*TimerIncreaseAmount/100f);
    }
}
