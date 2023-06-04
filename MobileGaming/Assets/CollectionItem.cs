using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class CollectionItem : ScriptableObject
{
    public Sprite itemSprite;
    [field: SerializeField] public Sprite emptySprite;
    public string objectTitle;
    public string chapterNumber;
    [field: SerializeField] public ItemRarity Rarity { get; private set; }
    [ResizableTextArea] public string descriptionText;
    [ResizableTextArea] public string powerUpText;
    [field: SerializeField] public List<ScriptableItemEffect> ScriptableItemEffects { get; private set; }
    [field: SerializeField] public List<Sprite> fragmentsSprites = new List<Sprite>();
    [field: SerializeField] public int FragmentCount { get; private set; }
    public int ObtainedFragment { get; private set; }
    public bool Completed =>  ObtainedFragment >= FragmentCount;
    
    [ContextMenu("Reset Progress")]
    public void InitProgress()
    {
        ObtainedFragment = 0;
        InvokeEvents();
    }

    public void GetProgress()
    {
        ObtainedFragment = 0;
        if (!PlayerPrefs.HasKey(name)) PlayerPrefs.SetInt(name, ObtainedFragment);
        ObtainedFragment = PlayerPrefs.GetInt(name);
        InvokeEvents();
    }

    private void SetProgress(int amount)
    {
        PlayerPrefs.SetInt(name, amount);
    }

    [ContextMenu("Fragment++")]
    public void ObtainFragment()
    {
        ObtainedFragment++;
        InvokeEvents();
    }

    public void ForceUnlock()
    {
        ObtainedFragment = FragmentCount;
        InvokeEvents();
    }

    private void InvokeEvents()
    {
        SetProgress(ObtainedFragment);
        EventManager.Trigger(new ObtainFragmentEvent(this,ObtainedFragment == FragmentCount));
    }
}

public enum ItemRarity {Rare,Epic,Legendary}

public class ObtainFragmentEvent
{
    public CollectionItem Item { get; }
    public bool Completed { get; }

    public ObtainFragmentEvent(CollectionItem item,bool completed)
    {
        Item = item;
        Completed = completed;
    }
}
