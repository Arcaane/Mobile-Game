using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Change Client Max Satisfaction")]
public class ScriptableItemIncreaseClientSatisfaction : ScriptableItemEffect
{
    [SerializeField,Tooltip("Percent of Base value")] private float satisfactionIncrease;
    
    protected override void Effect(LevelService levelService)
    {
        EventManager.AddListener<ClientDataSetEvent>(IncreaseClientSatisfaction);
    }

    protected override void CleanUp(LevelService levelService)
    {
        EventManager.RemoveListener<ClientDataSetEvent>(IncreaseClientSatisfaction);
    }

    private void IncreaseClientSatisfaction(ClientDataSetEvent clientDataSetEvent)
    {
        var slot = clientDataSetEvent.Slot;
        var data = slot.data;

        var increase = data.scriptableClient.BaseTimer * satisfactionIncrease;
        slot.IncreaseClientMaxSatisfaction(increase);
    }
}
