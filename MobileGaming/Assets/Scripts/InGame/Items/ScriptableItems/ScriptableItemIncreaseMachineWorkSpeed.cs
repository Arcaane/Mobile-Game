using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Change Machine Work Speed")]
public class ScriptableItemIncreaseMachineWorkSpeed : ScriptableItemEffect
{
    [field: SerializeField,Tooltip("Increases Speed by X of the Base Time")] public float TimeMultiplier { get; private set; } = 1f;
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
            var matchingShape = (shape & productShape) == productShape;
            var matchingColor = (color & productColor) == productColor;
            var matchingTopping = (topping & productTopping) == productTopping;
            
            var allMatch = (matchingShape && matchingColor && matchingTopping);
            
            if (!allMatch) return;

            var amount = machine.BaseTimeMultiplier*(TimeMultiplier-1);
            
            Debug.Log("Proc");
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
