using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenCollectionItem : MonoBehaviour
{
    [field:Header("Components")]
    public Image ItemImage { get; private set; }
    [SerializeField] private Button button;
    //public Image myLockImage;
    
    [SerializeField] public CollectionItem thisScriptable;
    [SerializeField] private ShowCollectionItemHolder _holder;
    [SerializeField] private Image fragmentImage;
    private List<GameObject> fragmentsGo = new List<GameObject>();
    
    public bool isUnlocked;

    private void Awake()
    {
        ItemImage = GetComponent<Image>();
        //myLockImage = transform.GetChild(0).GetComponent<Image>();
    }

    private void Start()
    {
        if (thisScriptable == null) return;
        
        ItemImage.sprite = thisScriptable.itemSprite;

        Debug.Log(fragmentImage,this);
        foreach (var sprite in thisScriptable.fragmentsSprites)
        {
            var image = Instantiate(fragmentImage, transform);
            image.sprite = sprite;
            fragmentsGo.Add(image.gameObject);
        }

        thisScriptable.OnObtainFragment += UpdateShownFragments;
        thisScriptable.OnCompleteFragment += Unlock;
    }

    private void Unlock()
    {
        SetUnlock(true);
    }
    
    private void SetUnlock(bool value)
    {
        isUnlocked = value;
        
        button.interactable = isUnlocked;
        ItemImage.color = isUnlocked ? Color.white : new Color(0.2f,0.2f,0.2f,1);
        
    }

    private void UpdateShownFragments()
    {
        var sprites = fragmentsGo.Count;
        float current = thisScriptable.ObtainedFragment;
        float total = thisScriptable.FragmentCount;
        if (total == 0) total = 1;
        Debug.Log(((current/total)*sprites));
        
        if(current < total) SetUnlock(false);
        foreach (var go in fragmentsGo)
        {
            go.SetActive(false);
        }
        for (int i = 0; i < ((current/total)*sprites); i++)
        {
            fragmentsGo[i].SetActive(true);
        }
    }
    
    

    public void ShowItem()
    {
        //_holder.FillAndShowItemCollectionDescription(thisScriptable);
    }
}
