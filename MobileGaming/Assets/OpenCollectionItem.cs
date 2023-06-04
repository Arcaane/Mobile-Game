using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenCollectionItem : MonoBehaviour
{
    [field:Header("Components")]
    public Image ItemImage { get; private set; }
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI fragmentProgressText;
    //public Image myLockImage;
    
    [SerializeField] public CollectionItem thisScriptable;
    [SerializeField] private Image fragmentImage;
    private List<GameObject> fragmentsGo = new List<GameObject>();
    
    public bool isUnlocked;

    private void Awake()
    {
        ItemImage = GetComponent<Image>();
    }

    private void Start()
    {
        if (thisScriptable == null) return;
        
        button.onClick.AddListener(ShowItem);
        ItemImage.sprite = thisScriptable.itemSprite;

        foreach (var sprite in thisScriptable.fragmentsSprites)
        {
            var image = Instantiate(fragmentImage, transform);
            image.sprite = sprite;
            fragmentsGo.Add(image.gameObject);
        }
    }
    
    private void OnEnable()
    {
        EventManager.AddListener<ObtainFragmentEvent>(UpdateShownFragments);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<ObtainFragmentEvent>(UpdateShownFragments);
    }
    
    private void UpdateShownFragments(ObtainFragmentEvent obtainFragmentEvent)
    {
        if(obtainFragmentEvent.Item != thisScriptable) return;
        SetUnlock(obtainFragmentEvent.Completed);
        
        var sprites = fragmentsGo.Count;
        float current = thisScriptable.ObtainedFragment;
        float total = thisScriptable.FragmentCount;
        if (total == 0) total = 1;
        
        foreach (var go in fragmentsGo)
        {
            go.SetActive(false);
        }
        
        for (int i = 0; i < Mathf.FloorToInt((current/total)*sprites); i++)
        {
            fragmentsGo[i].SetActive(true);
        }
        if(current > 0) fragmentsGo[0].SetActive(true);

        fragmentProgressText.text = $"{current}/{total}";
    }
    
    private void SetUnlock(bool value)
    {
        isUnlocked = value;
        
        button.interactable = isUnlocked;
        ItemImage.color = isUnlocked ? Color.white : new Color(0.2f,0.2f,0.2f,1);
        
    }
    
    private void ShowItem()
    {
        EventManager.Trigger(new ShowItemEvent(thisScriptable));
    }
}

public class ShowItemEvent
{
    public CollectionItem Item { get; }

    public ShowItemEvent(CollectionItem item)
    {
        Item = item;
    }
}
