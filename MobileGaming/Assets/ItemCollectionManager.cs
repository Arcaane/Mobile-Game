using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCollectionManager : MonoBehaviour
{
    public MainMenuManager menuManager;
    public ShowCollectionItemHolder showItem;
    public Canvas c1;
    public Canvas c2;
    public ScrollRect sr;
    
    public ItemSlot[] slots;
    public Sprite lockSprite;
    public Sprite emptySprite;
    
    Dictionary<string, int> itemSave = new();

    public OpenCollectionItem[] items;
    private void Start()
    {
        // Init dico
        for (int i = 0; i < items.Length; i++)
        {
            if (!PlayerPrefs.HasKey(items[i].name)) { PlayerPrefs.SetInt(items[i].name, 0); }
            itemSave.Add(items[i].name, PlayerPrefs.GetInt(items[i].name));
            items[i].isUnlocked = PlayerPrefs.GetInt(items[i].name) == 1;
        }
        
        UpdateCollectionSlots(menuManager.CollectionLevel);
        UpdateCollectionItemUnlocked();
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
                    slots[i].itemSlotImage.transform.parent.GetComponent<Button>().interactable = false;
                }
            }
            
            if (i < score)
            {
                slots[i].itemSlotImage.sprite = slots[i].itemInSlotSprite;
                slots[i].itemSlotImage.transform.parent.GetComponent<Button>().interactable = true;
            }
            else
            {
                slots[i].itemSlotImage.sprite = lockSprite;
                slots[i].itemSlotImage.transform.parent.GetComponent<Button>().interactable = false;
            }
            
        }
    }

    public void UpdateCollectionItemUnlocked()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].isUnlocked)
            {
                items[i].myLockImage.enabled = false;
                items[i].GetComponent<Button>().interactable = true;
                items[i].myItemImage.color = Color.white;
            }
            else
            {
                items[i].myItemImage.color = new Color(0.2f,0.2f,0.2f,1);
                items[i].GetComponent<Button>().interactable = false;
                items[i].myLockImage.enabled = true;
            }
        }
    }
    
    [ContextMenu("Unlock all")]
    public void UnlockedAllItems()
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i].isUnlocked = true;
            PlayerPrefs.SetInt(items[i].name, 1);
        }

        PlayerPrefs.Save();
        UpdateCollectionItemUnlocked();
    }
    
    [ContextMenu("Lock all")]
    public void LockAllItems()
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i].isUnlocked = false;
            PlayerPrefs.SetInt(items[i].name, 0);
        }

        PlayerPrefs.Save();
        UpdateCollectionItemUnlocked();
    }
    
    public void UnlockItem(int i)
    {
        items[i].isUnlocked = true;
        PlayerPrefs.SetInt(items[i].name, 1);
        PlayerPrefs.Save();
        UpdateCollectionItemUnlocked();
    }

    public void ShowItemInSlot(int i)
    {
        Debug.Log($"Slot {i} need a new item !");
        var tempItem = slots[i].item;
        if (tempItem == null) return;
        
        c1.gameObject.SetActive(false);
        c2.gameObject.SetActive(false);
        sr.enabled = false;
        showItem.gameObject.SetActive(true);
        showItem.FillAndShowItemCollectionDescription(tempItem);
    }
}

[Serializable]
public class ItemSlot
{
    public Image itemSlotImage;
    public Sprite itemInSlotSprite;
    public CollectionItem item;
}
