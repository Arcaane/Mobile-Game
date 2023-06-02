using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Change Line Work Speed When Crossed")]
public class ScriptableEffectIncreaseLinkSpeedIfColliding : ScriptableItemEffect
{
    [SerializeField,Tooltip("Speed Increase by X of the Base Time")] private float speedIncrease;
    private List<Action> actionsToCleanup = new ();
    
    protected override void Effect(LevelService levelService)
    {
        actionsToCleanup.Clear();
        EventManager.AddListener<LinkCollisionEvent>(IncreaseSpeedWhenCrossed);
    }

    protected override void CleanUp(LevelService levelService)
    {
        EventManager.RemoveListener<LinkCollisionEvent>(IncreaseSpeedWhenCrossed);
        for (int i = actionsToCleanup.Count - 1; i >= 0; i--)
        {
            actionsToCleanup[i].Invoke();
        }
    }

    private void IncreaseSpeedWhenCrossed(LinkCollisionEvent linkCollisionEvent)
    {
        var crossingLink = linkCollisionEvent.Link;
        foreach (var colliding in linkCollisionEvent.CollidingLinks)
        {
            colliding.IncreaseExtraSpeed(speedIncrease);
        }
        
        EventManager.AddListener<LinkDestroyedEvent>(RemoveSpeedOnLinkDestroyed);
        
        actionsToCleanup.Add(RemoveListener);
        
        void RemoveSpeedOnLinkDestroyed(LinkDestroyedEvent linkDestroyedEvent)
        {
            if(linkDestroyedEvent.Link != crossingLink) return;
            foreach (var colliding in linkCollisionEvent.CollidingLinks)
            {
                colliding.IncreaseExtraSpeed(-speedIncrease);
            }
            RemoveListener();
        }

        void RemoveListener()
        {
            actionsToCleanup.Remove(RemoveListener);
            EventManager.RemoveListener<LinkDestroyedEvent>(RemoveSpeedOnLinkDestroyed);
        }
    }
}
