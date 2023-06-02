using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemCollectionManager : MonoBehaviour
{
    public MainMenuManager menuManager;
    public ScrollRect sr;
    
    public ItemSlot[] slots;
    public ItemSlot[] showItemslots;
    public Sprite lockSprite;
    public Sprite emptySprite;
    
    public CollectionItem[] items;
    
    [Serializable]
    public class ItemSlot
    {
        [field:SerializeField] public Image ItemSlotImage { get; private set; }
        [field:SerializeField] public Button Button { get; private set; }
        [field:SerializeField] public Sprite LockedSprite { get; private set; }
        [field:SerializeField] public Sprite EmptySprite { get; private set; }
        public CollectionItem Item { get; private set; }
        private bool unlocked;

        public void Unlock(bool value)
        {
            unlocked = value;
            DisplayItem(Item);
        }
        
        public void DisplayItem(CollectionItem item)
        {
            Item = item;
            ItemSlotImage.sprite = Item != null ? Item.itemSprite : (unlocked ? EmptySprite : LockedSprite);
            if(Button != null) Button.interactable = Item != null;
        }
    }
    

    [ContextMenu("AutoFill items")]
    private void AutoFill()
    {
        items = GetComponentsInChildren<OpenCollectionItem>().Where(c => c.gameObject.activeSelf).Select(item => item.thisScriptable).ToArray();
    }
    
    private void Start()
    {
        for (var i = 0; i < slots.Length; i++)
        {
            var itemSlot = slots[i];
            var index = i;
            itemSlot.Unlock(false);
            itemSlot.DisplayItem(null);
            itemSlot.Button.onClick.AddListener(()=>ShowItemInSlot(index));
        }

        foreach (var itemSlot in showItemslots)
        {
            itemSlot.Unlock(false);
            itemSlot.DisplayItem(null);
        }

        GetProgress();
    }

    private void GetProgress()
    {
        foreach (var item in items)
        {
            item.GetProgress();
        }
        
        UnlockItemSlots(ScriptableItemDatabase.CollectionLevel);
    }
    
    private void OnEnable()
    {
        EventManager.AddListener<ShowItemEvent>(StopScroll);
        EventManager.AddListener<EquipItemEvent>(EquipItem);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<ShowItemEvent>(StopScroll);
        EventManager.RemoveListener<EquipItemEvent>(EquipItem);
    }

    public void StopScroll(ShowItemEvent showItemEvent)
    {
        sr.enabled = false;
    }
    
    [ContextMenu("Unlock all")]
    public void UnlockedAllItems()
    {
        foreach (var item in items)
        {
            item.ForceUnlock();
        }

        PlayerPrefs.Save();
    }
    
    [ContextMenu("Lock all")]
    public void LockAllItems()
    {
        foreach (var item in items)
        {
            item.InitProgress();
        }
        
        PlayerPrefs.Save();
    }

    public void UnlockItemSlots(int level)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Unlock(i < level);
            showItemslots[i].Unlock(i < level);
        }
    }

    public void EquipItem(EquipItemEvent equipItemEvent)
    {
        if(equipItemEvent.Slot > ScriptableItemDatabase.CollectionLevel) return;
        if(equipItemEvent.Slot < 0 || equipItemEvent.Slot >= slots.Length) return;
        
        slots[equipItemEvent.Slot].DisplayItem(equipItemEvent.Item);
        showItemslots[equipItemEvent.Slot].DisplayItem(equipItemEvent.Item);
    }

    public void ShowItemInSlot(int i)
    {
        EventManager.Trigger(new ShowItemEvent(slots[i].Item));
    }
}

public class EquipItemEvent
{
    public CollectionItem Item { get; }
    public int Slot { get; }

    public EquipItemEvent(CollectionItem item, int slot)
    {
        Item = item;
        Slot = slot;
    }
}
