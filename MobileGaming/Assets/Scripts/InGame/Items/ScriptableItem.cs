using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptableItem : ScriptableObject
{
    [field: SerializeField] private string Name;
    [field: SerializeField] private string Description;
    [field: SerializeField] private List<ItemFragment> fragments = new List<ItemFragment>();
    public List<ItemFragment> Fragments => fragments;

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
    public ScriptableItem AssociatedItem { get; private set; }

    public void SetItem(ScriptableItem item)
    {
        AssociatedItem = item;
    }

    public bool IsItem(ScriptableItem item)
    {
        return item == AssociatedItem;
    }
}
