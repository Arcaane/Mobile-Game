using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Increase Client Reward")]
public class ScriptableItemIncreaseClientReward : ScriptableItemEffect
{
    [SerializeField,Tooltip("Percent of Base value")] private float rewardIncrease;
    [SerializeField] private float minimumSatisfaction = 15f;
    private LevelService service;
    
    protected override void Effect(LevelService levelService)
    {
        service = levelService;
        EventManager.AddListener<ClientCompletedEvent>(IncreaseClientScoreOnThreshold);
    }

    protected override void CleanUp(LevelService levelService)
    {
        EventManager.RemoveListener<ClientCompletedEvent>(IncreaseClientScoreOnThreshold);
    }

    private void IncreaseClientScoreOnThreshold(ClientCompletedEvent clientCompletedEvent)
    {
        if(clientCompletedEvent.CurrentSatisfaction <= 0 || clientCompletedEvent.TimeToComplete > 15f) return;

        var increase = (int)Mathf.Ceil(clientCompletedEvent.Data.scriptableClient.BaseReward * rewardIncrease);
        service.IncreaseScore(increase);
    }
}
