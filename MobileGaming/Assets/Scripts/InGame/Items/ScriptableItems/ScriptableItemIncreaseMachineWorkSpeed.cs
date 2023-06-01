using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Effect/Change Machine Work Speed")]
public class ScriptableItemIncreaseMachineWorkSpeed : ScriptableItemEffect
{
    [SerializeField,Tooltip("Increases Time Speed by X of the Base Time")] public float timeMultiplier;
    [Header("Machine Work")]
    [SerializeField] private ProductShape shape;
    [SerializeField] private ProductColor color;
    [SerializeField] private ProductTopping topping;

    protected override void Effect(LevelService levelService)
    {
        EventManager.AddListener<MachineStartWorkEvent>(ChangeMultiplierOnStartWork);
    }

    protected override void CleanUp(LevelService levelService)
    {
        EventManager.RemoveListener<MachineStartWorkEvent>(ChangeMultiplierOnStartWork);
    }

    private void ChangeMultiplierOnStartWork(MachineStartWorkEvent startWorkData)
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

        var amount = machine.BaseSpeed*timeMultiplier;
            
        machine.IncreaseBonusSpeed(amount);
            
        EventManager.AddListener<MachineEndWorkEvent>(RemoveMultiplierOnEndWork);
            
        void RemoveMultiplierOnEndWork(MachineEndWorkEvent endWorkData)
        {
            machine.IncreaseBonusSpeed(-amount);
            EventManager.RemoveListener<MachineEndWorkEvent>(RemoveMultiplierOnEndWork);
        }
    }
}
