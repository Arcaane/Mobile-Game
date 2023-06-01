using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Increase Machine Speed On Delivery")]
public class ScriptableEffectIncreaseMachineSpeedOnDelivery : ScriptableItemEffect
{
    [SerializeField] private int requiredSuccesses = 0;
    [SerializeField] private float speedIncrease = 0;
    [SerializeField] private float effectDuration = 0;
    private Machine[] machines;
    private LevelService service;
    private int successes;
    private List<Action> actionsToCleanup = new ();

    protected override void Effect(LevelService levelService)
    {
        service = levelService;
        machines = service.CurrentLevel.machines.ToArray();
        successes = 0;
        EventManager.AddListener<DeliveryEvent>(IncreaseSuccess);
    }

    protected override void CleanUp(LevelService levelService)
    {
        EventManager.RemoveListener<DeliveryEvent>(IncreaseSuccess);
        for (int i = actionsToCleanup.Count - 1; i >= 0; i--)
        {
            actionsToCleanup[i].Invoke();
        }
    }

    private void IncreaseSuccess(DeliveryEvent deliveryEvent)
    {
        if(!deliveryEvent.Successful) return;
        successes++;
        if(successes < requiredSuccesses) return;
        IncreaseSpeed();
    }

    private void IncreaseSpeed()
    {
        foreach (var machine in machines)
        {
            IncreaseMachineSpeed(machine);
        }

        void IncreaseMachineSpeed(Machine machine)
        {
            var bonusTime = machine.BaseTimeToProduce * speedIncrease;
            machine.IncreaseBonusTime(bonusTime);
            
            var startTime = service.CurrentTime;
            
            EventManager.AddListener<LevelTimeUpdatedEvent>(DecreaseMachineSpeedOnDurationEnd);

            void DecreaseMachineSpeedOnDurationEnd(LevelTimeUpdatedEvent levelTimeUpdatedEvent)
            {
                if(levelTimeUpdatedEvent.CurrentTime - startTime < effectDuration) return;
                RemoveSpeed();
            }

            void RemoveSpeed()
            {
                actionsToCleanup.Remove(RemoveSpeed);
                machine.IncreaseBonusTime(-bonusTime);
                EventManager.RemoveListener<LevelTimeUpdatedEvent>(DecreaseMachineSpeedOnDurationEnd);
            }
            
            actionsToCleanup.Add(RemoveSpeed);
        }
    }
}
