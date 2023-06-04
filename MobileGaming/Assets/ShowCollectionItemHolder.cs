using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowCollectionItemHolder : MonoBehaviour
{
    [SerializeField] private GameObject panelGo;
    [SerializeField] private Button closeButton;
    public Image itemImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI chapterText;
    public TextMeshProUGUI raretyText;
    [SerializeField] private TextMeshProUGUI effectText;
    public GameObject[] GO;
    public GameObject equipItemPart;
    
    [SerializeField] private Button[] buttons;
    
    private CollectionItem displayedScriptable;

    private void Start()
    {
        ClosePanel();
        closeButton.onClick.AddListener(ClosePanel);
        for (int i = 0; i < buttons.Length; i++)
        {
            var index = i;
            buttons[i].onClick.AddListener(EquipOrUnequipItem);
            buttons[i].onClick.AddListener(ClosePanel);

            void EquipOrUnequipItem()
            {
                EquipOrUnequipItemInSlot(index);
            }
        }
    }

    private void ClosePanel()
    {
        panelGo.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.AddListener<ShowItemEvent>(FillAndShowItemCollectionDescription);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<ShowItemEvent>(FillAndShowItemCollectionDescription);
    }

    private void FillAndShowItemCollectionDescription(ShowItemEvent showItemEvent)
    {
        var itemScriptable = showItemEvent.Item;
        displayedScriptable = itemScriptable;
        itemImage.sprite = itemScriptable.itemSprite;
        titleText.text = itemScriptable.objectTitle;
        descriptionText.text = itemScriptable.descriptionText;
        chapterText.text = itemScriptable.chapterNumber;
        raretyText.text = itemScriptable.Rarity switch
        {
            ItemRarity.Rare => "Rare",
            ItemRarity.Epic => "Epic",
            ItemRarity.Legendary => "Legendary",
            _ => raretyText.text
        };
        effectText.text = itemScriptable.powerUpText;

        foreach (var t in GO)
        {
            t.SetActive(true);
        }
        
        equipItemPart.SetActive(true);
        
        if (ScriptableItemDatabase.CollectionLevel >= 1) buttons[0].interactable = true;
        if (ScriptableItemDatabase.CollectionLevel >= 2) buttons[1].interactable = true;
        if (ScriptableItemDatabase.CollectionLevel >= 3) buttons[2].interactable = true;
        
        panelGo.SetActive(true);
    }

    private void EquipOrUnequipItemInSlot(int i)
    {
        equipItemPart.SetActive(false);
        EventManager.Trigger(new EquipItemEvent(displayedScriptable,i));
    }
}
