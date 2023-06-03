using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "ItemDatabase")]
public class ScriptableItemDatabase : ScriptableObject
{
    [Serializable]
    public struct ItemsPerChapter
    {
        [field: SerializeField] public List<CollectionItem> items { get; private set; }
    }

    [Header("Currency")]
    [SerializeField] private int starCount;
    [SerializeField] private int goldCount;
    private static int collectionLevel;
    
    public int StarCount
    {
        get => starCount;
        set
        {
            starCount = value;
            PlayerPrefs.SetInt("Star", starCount);
            PlayerPrefs.Save();
            EventManager.Trigger(new StarValueChangedEvent(starCount));
        }
    }
    public int GoldCount
    {
        get => goldCount;
        set
        {
            goldCount = value;
            PlayerPrefs.SetInt("Gold", goldCount);
            PlayerPrefs.Save();
            EventManager.Trigger(new GoldValueChangedEvent(goldCount));
        }
    }
    public static int CollectionLevel
    {
        get => collectionLevel;
        set
        {
            collectionLevel = value;
            PlayerPrefs.SetInt("CollectionLevel", collectionLevel);
            PlayerPrefs.Save();
            EventManager.Trigger(new CollectionLevelChangeEvent(collectionLevel));
        }
    }
    
    [Header("Items")]
    [SerializeField] private List<CollectionItem> allItems = new();

    [Header("Gacha")]
    [SerializeField] private int wishCost;
    [SerializeField] private List<ItemsPerChapter> itemsPerChapter = new ();
    [SerializeField] private List<CollectionItem> itemPool = new ();

    [SerializeField,ReadOnly] private int completedChapters = 0;
    [SerializeField,ReadOnly] private int unlockedLevels = 1;

    public void SetListeners()
    {
        EventManager.AddListener<GainStarEvent>(IncreaseTotalStars);
        EventManager.AddListener<GainScoreEvent>(IncreaseTotalGold);
        EventManager.AddListener<RefreshSagaMapLevelsEvent>(CheckUnlockedChapters);

        void IncreaseTotalStars(GainStarEvent gainStarEvent)
        {
            StarCount += gainStarEvent.Amount;
            Debug.Log($"Gained {gainStarEvent.Amount} stars, now at {StarCount}");
        }
        
        void IncreaseTotalGold(GainScoreEvent gainScoreEvent)
        {
            GoldCount += gainScoreEvent.Amount;
            Debug.Log($"Gained {gainScoreEvent.Amount} stars, now at {GoldCount}");
        }
        
        void CheckUnlockedChapters(RefreshSagaMapLevelsEvent refreshSagaMapLevelsEvent)
        {
            foreach (var scriptableLevel in refreshSagaMapLevelsEvent.ScriptableLevelsInSagaMap.Where(scriptableLevel => scriptableLevel.Completed && scriptableLevel.IsLastLevelOfChapter))
            {
                SetChapterCompleted(scriptableLevel.CurrentChapter);
            }

            RefreshGachaPool();
        }
    }

    [ContextMenu("Get Progress")]
    public void GetProgress()
    {
        if (!PlayerPrefs.HasKey("Star")) PlayerPrefs.SetInt("Star", 0);
        if (!PlayerPrefs.HasKey("Gold")) PlayerPrefs.SetInt("Gold", 0);
        if (!PlayerPrefs.HasKey("CollectionLevel")) PlayerPrefs.SetInt("CollectionLevel", 0);
        StarCount = PlayerPrefs.GetInt("Star");
        GoldCount = PlayerPrefs.GetInt("Gold");
        CollectionLevel = PlayerPrefs.GetInt("CollectionLevel");
        
        foreach (var item in allItems)
        {
            item.GetProgress();
        }
        
        if (!PlayerPrefs.HasKey("CompletedChapters")) PlayerPrefs.SetInt("CompletedChapters", 0);
        completedChapters = PlayerPrefs.GetInt("CompletedChapters");
        
        if (!PlayerPrefs.HasKey("LevelUnlocked")) PlayerPrefs.SetInt("LevelUnlocked", 1);
        unlockedLevels = PlayerPrefs.GetInt("LevelUnlocked");
        if (unlockedLevels < 0)
        {
            PlayerPrefs.SetInt("LevelUnlocked", 1);
            unlockedLevels = 1;
        }

        RefreshGachaPool();
    }

    private void RefreshGachaPool()
    {
        itemPool.Clear();
        if(completedChapters <= 0) return;
        for (int i = 1; i < completedChapters+1; i++)
        {
            AddChapterToGacha(i);
        }
    }

    public void SetLevelUnlocked(int amount)
    {
        unlockedLevels = amount;
        PlayerPrefs.SetInt("LevelUnlocked", unlockedLevels);
        PlayerPrefs.Save();
    }

    private void SetChapterCompleted(int amount)
    {
        completedChapters = amount;
        PlayerPrefs.SetInt("CompletedChapters", completedChapters);
        PlayerPrefs.Save();
    }
    
    public void AddChapterToGacha(int chapter)
    {
        chapter--; // index starts at 0, but chapter 1 is 1
        foreach (var item in itemsPerChapter[chapter].items.Where(item => item.ObtainedFragment < item.FragmentCount))
        {
            for (int i = 0; i < (item.FragmentCount-item.ObtainedFragment); i++)
            {
                itemPool.Add(item);
            }
        }
    }
    
    private (CollectionItem item, int gold) Pull()
    {
        if (itemPool.Count <= 0)
        {
            Debug.LogWarning("Empty Pool");
            return (null,0);
        }

        var index = Random.Range(0, itemPool.Count);
        var item = itemPool[index];
        itemPool.RemoveAt(index);
        return (item,0);
    }
    
    [ContextMenu("Wish")]
    public void Wish()
    {
        if (StarCount - wishCost < 0) return;
        
        StarCount -= wishCost;
        
        var (item, gold) = Pull();
        
        EventManager.Trigger(new WishEvent(item,gold));

        if (item == null) return;

        item.ObtainFragment();
    }

    [ContextMenu("Add")]
    private void AddChapter()
    {
        AddChapterToGacha(0);
    }
    
    [ContextMenu("Reset All Items")]
    public void LockAllItems()
    {
        foreach (var item in allItems)
        {
            item.InitProgress();
        }
        
        PlayerPrefs.Save();
    }
}

public class WishEvent
{
    public CollectionItem Item { get; }
    public int Gold { get; }

    public WishEvent(CollectionItem item, int gold)
    {
        Item = item;
        Gold = gold;
    }
}

public class GoldValueChangedEvent
{
    public int Value { get; }

    public GoldValueChangedEvent(int value)
    {
        Value = value;
    }
}

public class StarValueChangedEvent
{
    public int Value { get; }

    public StarValueChangedEvent(int value)
    {
        Value = value;
    }
}

public class CollectionLevelChangeEvent
{
    public int Value { get; }

    public CollectionLevelChangeEvent(int value)
    {
        Value = value;
    }
}

