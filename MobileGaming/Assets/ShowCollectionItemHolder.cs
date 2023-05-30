using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowCollectionItemHolder : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI chapterText;
    public TextMeshProUGUI raretyText;
    public GameObject[] GO;
    public GameObject equipItemPart;
    public ItemCollectionManager itemCollectionManager;
    
    [SerializeField] private Image[] items;
    [SerializeField] private Button[] buttons;
    
    private CollectionItem lastScriptableOpened;
    public void FillAndShowItemCollectionDescription(CollectionItem itemScriptable)
    {
        lastScriptableOpened = itemScriptable;
        
        itemImage.sprite = itemScriptable.itemSprite;
        titleText.text = itemScriptable.objectTitle;
        descriptionText.text = itemScriptable.descriptionText;
        chapterText.text = itemScriptable.chapterNumber;
        raretyText.text = itemScriptable.Rarity switch
        {
            ItemRarity.Rare => "Rare",
            ItemRarity.Epic => "Epic",
            ItemRarity.Legendary => "Legendary",
            _ => raretyText.text
        };

        foreach (var t in GO)
        {
            t.SetActive(true);
        }

        if (itemScriptable.isEquiped) return;
        
        equipItemPart.SetActive(true);

        for (int i = 0; i < itemCollectionManager.slots.Length; i++)
        {
            items[i].sprite = itemCollectionManager.slots[i].itemSlotImage.sprite;
        }

        if (itemCollectionManager.menuManager.CollectionLevel == 1) buttons[0].interactable = true;
        if (itemCollectionManager.menuManager.CollectionLevel == 2) buttons[1].interactable = true;
        if (itemCollectionManager.menuManager.CollectionLevel == 2) buttons[2].interactable = true;
    }

    public void SetItemInSlot(int i)
    {
        if (itemCollectionManager.slots[i].item != null) itemCollectionManager.slots[i].item.isEquiped = false;
        
        lastScriptableOpened.isEquiped = true;
        itemCollectionManager.slots[i].item = lastScriptableOpened;
        itemCollectionManager.slots[i].itemInSlotSprite = lastScriptableOpened.itemSprite;
        equipItemPart.SetActive(false);
        
        itemCollectionManager.UpdateCollectionSlots(itemCollectionManager.menuManager.CollectionLevel);
    }
}
