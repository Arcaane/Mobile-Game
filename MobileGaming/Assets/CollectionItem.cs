using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class CollectionItem : ScriptableObject
{
    public Sprite itemSprite;
    public string objectTitle;
    public string chapterNumber;
    [field: SerializeField] public ItemRarity Rarity { get; private set; }
    [ResizableTextArea] public string descriptionText;
    [ResizableTextArea] public string powerUpText;
    [field: SerializeField] public ScriptableItemEffect ScriptableItemEffect { get; private set; }
    [field: SerializeField] private List<ItemFragment> fragments = new List<ItemFragment>();
    public List<ItemFragment> Fragments => fragments;
    public bool isEquiped;

}

public enum ItemRarity {Rare,Epic,Legendary}
