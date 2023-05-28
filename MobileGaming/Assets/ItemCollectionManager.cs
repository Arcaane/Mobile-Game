using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCollectionManager : MonoBehaviour
{
    public MainMenuManager menuManager;
    public ItemSlot[] slots;
    public Sprite lockSprite;
    public Sprite emptySprite;

    public event Action<int> OnCollectionScoreChange;

    private void Start()
    {
        OnCollectionScoreChange = null;
    }

    private void UpdateCollectionSlots(int score)
    {
        for (int i = 0; i < score; i++)
        {
            
        }
    }
}

[Serializable]
public class ItemSlot
{
    public Image itemSlotImage;
    public Sprite itemInSlotSprite;
}
