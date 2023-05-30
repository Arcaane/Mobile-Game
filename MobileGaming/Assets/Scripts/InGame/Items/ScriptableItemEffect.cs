using System;
using UnityEngine;

public abstract class ScriptableItemEffect : ScriptableObject
{
    public void ActivateEffect()
    {
        Effect();
    }

    protected abstract void Effect();
}

[Serializable]
public class ItemFragment
{
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public float BaseWeight { get; private set; } = 1f;
    public CollectionItem AssociatedItem { get; private set; }

    public void SetItem(CollectionItem itemEffect)
    {
        AssociatedItem = itemEffect;
    }

    public bool IsItem(CollectionItem itemEffect)
    {
        return itemEffect == AssociatedItem;
    }
}
