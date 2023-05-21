using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "ItemDatabase")]
public class ScriptableItemDatabase : ScriptableObject
{
    [Serializable]
    public struct ItemsPerLevel
    {
        [field: SerializeField] public List<ScriptableItem> items { get; private set; }
    }
    
    [SerializeField] private List<ItemsPerLevel> allItems = new ();
    [SerializeField] private List<ItemFragment> availableFragments = new ();
    [SerializeField] private List<ItemFragment> obtainedFragments = new ();
    [SerializeField] private List<ScriptableItem> obtainedItems = new ();
    
    private int currency;
    public int Currency => currency;
    
    public void AddItemToGacha(params ScriptableItem[] items)
    {
        foreach (var item in items)
        {
            foreach (var fragment in item.Fragments.Where(fragment => !availableFragments.Contains(fragment)))
            {
                fragment.SetItem(item);
                availableFragments.Add(fragment);
            }
        }
    }

    public void GetRandomFragment()
    {
        if (availableFragments.Count <= 0)
        {
            Debug.LogWarning("No fragments available");
            GainCurrency(10);
            return;
        }
        var index = Random.Range(0, availableFragments.Count);
        var fragment = availableFragments[index];

        obtainedFragments.Add(fragment);
        availableFragments.Remove(fragment);
        
        TryGetItem(fragment.AssociatedItem);
    }

    private void TryGetItem(ScriptableItem item)
    {
        if(obtainedItems.Contains(item)) return;

        if (item.Fragments.Count < ObtainedFragments(item)) return;

        GetItem(item);
    }

    private void GetItem(ScriptableItem item)
    {
        obtainedItems.Add(item);
    }

    public int ObtainedFragments(ScriptableItem item)
    {
        return obtainedFragments.Count(fragment => fragment.AssociatedItem == item);
    }

    public void GainCurrency(int amount)
    {
        currency += amount;
    }

    public void SpendCurrency(int amount)
    {
        currency -= amount;
    }
}
