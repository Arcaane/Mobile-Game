using UnityEngine;
using UnityEngine.UI;

public class OpenCollectionItem : MonoBehaviour
{
    private Image myItemImage;
    [SerializeField] private CollectionItem thisScriptable;
    [SerializeField] private ShowCollectionItemHolder _holder;
        
    private void Start()
    {
        myItemImage = GetComponent<Image>();
        if (thisScriptable != null)
        {
            myItemImage.sprite = thisScriptable.itemSprite;
        }
    }

    public void ShowItem()
    {
        _holder.FillAndShowItemCollectionDescription(thisScriptable.itemSprite, 
            thisScriptable.objectTitle, 
            thisScriptable.descriptionText, 
            thisScriptable.chapterNumber, 
            thisScriptable.Rarity);
    }
}
