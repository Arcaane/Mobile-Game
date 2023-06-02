using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

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
    
    public bool isEquiped;
    public event Action OnObtainFragment;
    public event Action OnCompleteFragment;
    
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

    public void SetProgess(int amount)
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
        SetProgess(ObtainedFragment);
        OnObtainFragment?.Invoke();
        if(ObtainedFragment == FragmentCount) OnCompleteFragment?.Invoke();
    }
}

public enum ItemRarity {Rare,Epic,Legendary}
