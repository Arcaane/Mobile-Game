using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Change Line Collision Slow")]
public class ScriptableEffectDecreaseCollisionSlow : ScriptableItemEffect
{
    [SerializeField,Tooltip("In X of the Base Time")] private float collisionSpeed;
    protected override void Effect(LevelService levelService)
    {
        EventManager.AddListener<LinkCreatedEvent>(ChangeCollisionSlow);
    }

    protected override void CleanUp(LevelService levelService)
    {
        EventManager.RemoveListener<LinkCreatedEvent>(ChangeCollisionSlow);
    }

    private void ChangeCollisionSlow(LinkCreatedEvent linkCreatedEvent)
    {
        var link = linkCreatedEvent.Link;
        var speed = collisionSpeed - link.BaseCollidingLinksSlowAmount; 
        link.IncreaseExtraTimeInCollision(speed);
    }
}
