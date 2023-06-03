using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Settings")]
public class ScriptableSettings : ScriptableObject
{
    [field:Header("Sprites")]
    [field: SerializeField] public Sprite[] bottleShapesSprites { get; private set; }
    [field: SerializeField] public Sprite[] bottleContentSprites { get; private set; }
    
    [field:Header("Items")]
    [field: SerializeField] public ScriptableItemDatabase itemDB { get; private set; }
    [field: SerializeField] private List<ScriptableItemEffect> equippedItemEffects = new List<ScriptableItemEffect>();
    public static List<ScriptableItemEffect> EquippedItemEffects => GlobalSettings.equippedItemEffects;
    public static ScriptableSettings GlobalSettings { get; private set; }

    public void SetAsGlobalSettings()
    {
        GlobalSettings = this;
        equippedItemEffects.Clear();
    }

    public static void EquipItem(CollectionItem item)
    {
        GlobalSettings.equippedItemEffects.AddRange(item.ScriptableItemEffects);
    }
    
    public static void RemoveItem(CollectionItem item)
    {
        foreach (var effect in item.ScriptableItemEffects.Where(effect => GlobalSettings.equippedItemEffects.Contains(effect)))
        {
            GlobalSettings.equippedItemEffects.Remove(effect);
        }
    }
    
    [ContextMenu("Reset")]
    private void Reset()
    {
        GlobalSettings.equippedItemEffects.Clear();
    }
}
