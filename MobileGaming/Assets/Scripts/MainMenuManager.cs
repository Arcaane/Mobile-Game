using System;
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

    public int StarCount
    {
        get => starCount;
        set
        {
            starCount = value;
            EventManager.Trigger(new StarValueChangedEvent(starCount));
        }
    }
    public int GoldCount
    {
        get => goldCount;
        set
        {
            goldCount = value;
            EventManager.Trigger(new GoldValueChangedEvent(goldCount));
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("Star")) PlayerPrefs.SetInt("Star", 0);
        if (!PlayerPrefs.HasKey("Gold")) PlayerPrefs.SetInt("Gold", 0);
        if (!PlayerPrefs.HasKey("CollectionLevel")) PlayerPrefs.SetInt("CollectionLevel", 0);

        StarCount = PlayerPrefs.GetInt("Star");
        GoldCount = PlayerPrefs.GetInt("Gold");

        settingsMenu.SetActive(isInSettingsMenu);
    }

    private void OnEnable()
    {
        EventManager.AddListener<GoldValueChangedEvent>(UpdateGoldText);
        EventManager.AddListener<StarValueChangedEvent>(UpdateStarText);
        EventManager.AddListener<CollectionLevelChangeEvent>(UpdateItemLockState);
        _collectionManager.UnlockItemSlots(ScriptableItemDatabase.CollectionLevel);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<GoldValueChangedEvent>(UpdateGoldText);
        EventManager.RemoveListener<StarValueChangedEvent>(UpdateStarText);
        EventManager.RemoveListener<CollectionLevelChangeEvent>(UpdateItemLockState);
    }
    
    public void ToggleSettings()
    {
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

    #region PlayerMethods
    public void AddStar(int i)
    {
        StarCount += i;
        PlayerPrefs.SetInt("Star", starCount);
        PlayerPrefs.Save();
    }
    public void SubtractStar(int i)
    {
        StarCount -= i;
        PlayerPrefs.SetInt("Star", starCount);
        PlayerPrefs.Save();
    }
    public void AddGold(int i)
    {
        GoldCount += i;
        PlayerPrefs.SetInt("Gold", goldCount);
        PlayerPrefs.Save();
    }
    public void SubtractGold(int i)
    {
        GoldCount -= i;
        PlayerPrefs.SetInt("Gold", goldCount);
        PlayerPrefs.Save();
    }

    public void LevelCollectionButton(int i) =>  ScriptableItemDatabase.CollectionLevel = i;
    #endregion
    
    #region UIMethods
    
    public void UpdateItemLockState(CollectionLevelChangeEvent collectionLevelChangeEvent)
    {
        Debug.Log($"Changed collection level to {collectionLevelChangeEvent.Value}");
        _collectionManager.UnlockItemSlots(collectionLevelChangeEvent.Value);
    }

    public void UpdateGoldText(GoldValueChangedEvent goldValueChangedEvent)
    {
        goldCountText.text = $"{goldValueChangedEvent.Value}";
    }
    
    public void UpdateStarText(StarValueChangedEvent starValueChangedEvent)
    {
        starCountText.text = $"{starValueChangedEvent.Value}";
    }
    
    #endregion
}

