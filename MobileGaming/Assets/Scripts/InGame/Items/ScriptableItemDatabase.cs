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
        [field: SerializeField] public List<ScriptableItemEffect> items { get; private set; }
    }
    
    [SerializeField] private List<ItemsPerLevel> allItems = new ();
    [SerializeField] private List<ItemFragment> availableFragments = new ();
    [SerializeField] private List<ItemFragment> obtainedFragments = new ();
    [SerializeField] private List<CollectionItem> obtainedItems = new ();
    
    private int currency;
    public int Currency => currency;
    
    public void AddItemToGacha(params CollectionItem[] items)
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

    private void TryGetItem(CollectionItem itemEffect)
    {
        if(obtainedItems.Contains(itemEffect)) return;

        if (itemEffect.Fragments.Count < ObtainedFragments(itemEffect)) return;

        GetItem(itemEffect);
    }

    private void GetItem(CollectionItem itemEffect)
    {
        obtainedItems.Add(itemEffect);
    }

    public int ObtainedFragments(CollectionItem itemEffect)
    {
        return obtainedFragments.Count(fragment => fragment.AssociatedItem == itemEffect);
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
