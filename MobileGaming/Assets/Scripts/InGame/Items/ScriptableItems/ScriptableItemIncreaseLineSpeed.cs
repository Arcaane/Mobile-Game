using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Change Line Work Speed")]
public class ScriptableItemIncreaseLineSpeed : ScriptableItemEffect
{
    [field: SerializeField,Tooltip("Increases Speed by X of the Base Time")] public float TimeMultiplier { get; private set; } = 1f;
    [Header("Start Machine Work")]
    [SerializeField] private ProductShape shape;
    [SerializeField] private ProductColor color;
    [SerializeField] private ProductTopping topping;
    
    protected override void Effect(LevelService levelService)
    {
        EventManager.AddListener<LinkCreatedEvent>(ChangeMultiplierLinkCreated);
        
        void ChangeMultiplierLinkCreated(LinkCreatedEvent createdEvent)
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
            
            
            var amount = link.BaseTimeToCompleteTransportation*(TimeMultiplier-1);
            
            link.IncreaseExtraTimeToComplete(amount);
        }
    }
}
