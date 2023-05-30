using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Change Machine Work Speed")]
public class ScriptableItemIncreaseMachineWorkSpeed : ScriptableItemEffect
{
    private enum ComparisonMode
    {
        And = 0,
        Or = 1,
    }
    
    [field: SerializeField,Tooltip("Increases Speed by X of the Base Time")] public float TimeMultiplier { get; private set; } = 1f;
    [SerializeField] private ComparisonMode comparison;
    [SerializeField] private ProductShape shape;
    [SerializeField] private ProductColor color;
    [SerializeField] private ProductTopping topping;

    protected override void Effect(LevelService levelService)
    {
        EventManager.AddListener<MachineStartWorkEvent>(ChangeMultiplierOnStartWork);
        
        void ChangeMultiplierOnStartWork(MachineStartWorkEvent startWorkData)
        {
            var machine = startWorkData.Machine;
            var productShape = machine.MachineShape;
            var productColor = machine.MachineColor;
            var productTopping = machine.machineTopping;
            var matchingShape = (shape & productShape) != productShape;
            var matchingColor = (color & productColor) != productColor;
            var matchingTopping = (topping & productTopping) != productTopping;
            
            if ((int)comparison == 1
                    ? (matchingShape || matchingColor || matchingTopping)
                    : (matchingShape && matchingColor && matchingTopping)) return;

            var amount = machine.BaseTimeMultiplier*(TimeMultiplier-1);
            
            machine.IncreaseTimeMultiplier(amount);
            
            EventManager.AddListener<MachineEndWorkEvent>(RemoveMultiplierOnEndWork);
            
            void RemoveMultiplierOnEndWork(MachineEndWorkEvent endWorkData)
            {
                machine.IncreaseTimeMultiplier(-amount);
                EventManager.RemoveListener<MachineEndWorkEvent>(RemoveMultiplierOnEndWork);
            }
        }
        
    }
}
