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

    private void Start()
    {
        UpdateCollectionSlots(menuManager.CollectionLevel);
    }

    public void UpdateCollectionSlots(int score)
    {
        Debug.Log( $"Collection score {score}");
        for (int i = 0; i < slots.Length; i++)
        {
            if (score == 0)
            {
                for (int j = 0; j < slots.Length; j++)
                {
                    slots[i].itemSlotImage.sprite = lockSprite;
                }
            }
            
            if (i < score)
            {
                slots[i].itemSlotImage.sprite = slots[i].itemInSlotSprite;
            }
            else
            {
                slots[i].itemSlotImage.sprite = lockSprite;
            }
            
        }
    }
}

[Serializable]
public class ItemSlot
{
    public Image itemSlotImage;
    public Sprite itemInSlotSprite;
}
