using Service;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Settings")]
    public Button settingsButton;
    public bool isInSettingsMenu;
    public GameObject settingsMenu;
    [SerializeField] private ItemCollectionManager _collectionManager;

    [Space(10)] [Header("User variables")] 
    [SerializeField] private TextMeshProUGUI starCountText;
    [SerializeField] private TextMeshProUGUI goldCountText;
    
    private int starCount;
    private int goldCount;
    
    private void Start()
    {
        if (!PlayerPrefs.HasKey("Star")) PlayerPrefs.SetInt("Star", 0);
        if (!PlayerPrefs.HasKey("Gold")) PlayerPrefs.SetInt("Gold", 0);
        if (!PlayerPrefs.HasKey("CollectionLevel")) PlayerPrefs.SetInt("CollectionLevel", 0);

        starCount = PlayerPrefs.GetInt("Star");
        goldCount = PlayerPrefs.GetInt("Gold");
        
        starCountText.text = $"{starCount}";
        goldCountText.text = $"{goldCount}";

        settingsMenu.SetActive(isInSettingsMenu);
    }

    private void OnEnable()
    {
        EventManager.AddListener<GoldValueChangedEvent>(UpdateGoldText);
        EventManager.AddListener<StarValueChangedEvent>(UpdateStarText);
        EventManager.AddListener<CollectionLevelChangeEvent>(UpdateItemLockState);
        _collectionManager.UnlockItemSlots(ScriptableItemDatabase.CollectionLevel);
        starCountText.text = $"{starCount}";
        goldCountText.text = $"{goldCount}";
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<GoldValueChangedEvent>(UpdateGoldText);
        EventManager.RemoveListener<StarValueChangedEvent>(UpdateStarText);
        EventManager.RemoveListener<CollectionLevelChangeEvent>(UpdateItemLockState);
    }
    
    public void ToggleSettings()
    {
        return;
        isInSettingsMenu = !isInSettingsMenu;
        settingsMenu.SetActive(isInSettingsMenu);
    }

    public void ToggleGamePath()
    {
        if (isInSettingsMenu) ToggleSettings();
    }

    public void PlaceHolderGoToGameScene(int level)
    {
        Debug.Log(level);
        GameService.LoadLevel(level);
    }
    
    public void AddStar(int i)
    {
        starCount += i;
        PlayerPrefs.SetInt("Star", starCount);
        PlayerPrefs.Save();
    }
    
    public void SubtractStar(int i)
    {
        starCount -= i;
        PlayerPrefs.SetInt("Star", starCount);
        PlayerPrefs.Save();
    }
    public void AddGold(int i)
    {
        goldCount += i;
        PlayerPrefs.SetInt("Gold", goldCount);
        PlayerPrefs.Save();
    }
    public void SubtractGold(int i)
    {
        goldCount -= i;
        PlayerPrefs.SetInt("Gold", goldCount);
        PlayerPrefs.Save();
    }

    public void LevelCollectionButton(int i) =>  ScriptableItemDatabase.CollectionLevel = i;

    #region UIMethods
    
    private void UpdateItemLockState(CollectionLevelChangeEvent collectionLevelChangeEvent)
    {
        _collectionManager.UnlockItemSlots(collectionLevelChangeEvent.Value);
    }

    private void UpdateGoldText(GoldValueChangedEvent goldValueChangedEvent)
    {
        goldCountText.text = $"{goldValueChangedEvent.Value}";
    }
    
    private void UpdateStarText(StarValueChangedEvent starValueChangedEvent)
    {
        starCountText.text = $"{starValueChangedEvent.Value}";
    }
    
    #endregion
}

