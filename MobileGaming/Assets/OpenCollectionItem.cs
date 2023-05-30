using UnityEngine;
using UnityEngine.UI;

public class OpenCollectionItem : MonoBehaviour
{
    public Image myItemImage;
    public Image myLockImage;
    
    [SerializeField] public CollectionItem thisScriptable;
    [SerializeField] private ShowCollectionItemHolder _holder;
    
    public bool isUnlocked;

    private void Awake()
    {
        myItemImage = GetComponent<Image>();
        myLockImage = transform.GetChild(0).GetComponent<Image>();
    }

    private void Start()
    {
        if (thisScriptable != null) myItemImage.sprite = thisScriptable.itemSprite;
    }

    public void ShowItem()
    {
        _holder.FillAndShowItemCollectionDescription(thisScriptable);
    }
}
