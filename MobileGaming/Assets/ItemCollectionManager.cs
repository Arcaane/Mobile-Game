using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemCollectionManager : MonoBehaviour
{
    public ScrollRect sr;
    
    public ItemSlot[] slots;
    public ItemSlot[] showItemslots;

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
        EventManager.Trigger(new RefreshEquippedEvent());
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
        EventManager.AddListener<UnequipItemEvent>(UnequipItem);
        EventManager.Trigger(new RefreshEquippedEvent());
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<ShowItemEvent>(StopScroll);
        EventManager.RemoveListener<EquipItemEvent>(EquipItem);
        EventManager.RemoveListener<UnequipItemEvent>(UnequipItem);
    }

    private void StopScroll(ShowItemEvent showItemEvent)
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

    private void EquipItem(EquipItemEvent equipItemEvent)
    {
        var index = equipItemEvent.Slot;
        if(index > ScriptableItemDatabase.CollectionLevel) return;
        if(index < 0 || index >= slots.Length) return;
        
        var slot = slots[index];
        if(slot.Item != null) EventManager.Trigger(new UnequipItemEvent(slot.Item,index));
        
        slots[index].DisplayItem(equipItemEvent.Item);
        showItemslots[index].DisplayItem(equipItemEvent.Item);
    }

    private void UnequipItem(UnequipItemEvent unequipItemEvent)
    {
        var index = unequipItemEvent.Slot;
        if(index < 0 || index >= slots.Length) return;
        
        slots[index].DisplayItem(null);
        showItemslots[index].DisplayItem(null);
    }

    private void ShowItemInSlot(int i)
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

public class UnequipItemEvent
{
    public CollectionItem Item { get; }
    public int Slot { get; }

    public UnequipItemEvent(CollectionItem item, int slot)
    {
        Item = item;
        Slot = slot;
    }
}

public class RefreshEquippedEvent
{
    
}
