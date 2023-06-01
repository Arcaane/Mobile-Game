using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Change Line Work Speed")]
public class ScriptableItemIncreaseLineSpeed : ScriptableItemEffect
{
    [SerializeField,Tooltip("Decreases Line Duration by X of the Base Time")] private float decreasedTime;
    [SerializeField] private int maxUses = 0;
    private int currentUses;
    [Header("Start Machine Work")]
    [SerializeField] private ProductShape shape;
    [SerializeField] private ProductColor color;
    [SerializeField] private ProductTopping topping;
    
    protected override void Effect(LevelService levelService)
    {
        currentUses = 0;
        EventManager.AddListener<LinkCreatedEvent>(ChangeMultiplierLinkCreated);
    }

    protected override void CleanUp(LevelService levelService)
    {
        EventManager.RemoveListener<LinkCreatedEvent>(ChangeMultiplierLinkCreated);
    }
    
    private void ChangeMultiplierLinkCreated(LinkCreatedEvent createdEvent)
    {
        var link = createdEvent.Link;
            
        if(link.StartLinkable is not Machine machine) return;
            
        var productShape = machine.MachineShape;
        var productColor = machine.MachineColor;
        var productTopping = machine.machineTopping;
        var matchingShape = (shape & productShape) == productShape;
        var matchingColor = (color & productColor) == productColor;
        var matchingTopping = (topping & productTopping) == productTopping;
            
        var allMatch = (matchingShape && matchingColor && matchingTopping);
            
        if (!allMatch) return;
            
        var amount = link.BaseTimeToCompleteTransportation*decreasedTime;
        link.IncreaseExtraSpeed(amount);
        
        if(maxUses <= 0) return;
        
        currentUses++;
        if(currentUses < maxUses) return;
        
        EventManager.RemoveListener<LinkCreatedEvent>(ChangeMultiplierLinkCreated);
    }
}
