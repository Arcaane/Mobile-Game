using System;
using Service;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class MainMenuManager : MonoBehaviour
{
    [Header("Settings")]
    public Button settingsButton;
    public bool isInSettingsMenu;
    public GameObject settingsMenu;
    [SerializeField] private ScriptableSettings settings;

    public RectTransform levelPanelRectTransform;

    [Space(10)] [Header("User variables")] 
    [SerializeField] private TextMeshProUGUI starCountText;
    [SerializeField] private TextMeshProUGUI goldCountText;
    
    private int starCount;
    private int goldCount;
    private int collectionLevel;
    
    [field:SerializeField] 
    public int StarCount
    {
        get => starCount;
        set
        {
            starCount = value;
            OnStarChangeValue?.Invoke(value);
        }
    }
    
    [field:SerializeField] 
    public int GoldCount
    {
        get => goldCount;
        set
        {
            goldCount = value;
            OnGoldChangeValue?.Invoke(value);
        }
    }

    [field: SerializeField]
    public int CollectionLevel
    {
        get => collectionLevel;
        set
        {
            collectionLevel = value;
            OnCollectionLevelChange?.Invoke(value);
        }
    }
     
    // Start is called before the first frame update
    void Start()
    {
        InitUIAction();
        
        if (!PlayerPrefs.HasKey("Star")) PlayerPrefs.SetInt("Star", StarCount);
        if (!PlayerPrefs.HasKey("Gold")) PlayerPrefs.SetInt("Gold", GoldCount);
        if (!PlayerPrefs.HasKey("CollectionLevel")) PlayerPrefs.SetInt("CollectionLevel", 0);

        StarCount = PlayerPrefs.GetInt("Star");
        GoldCount = PlayerPrefs.GetInt("Gold");
        GoldCount = PlayerPrefs.GetInt("Gold");
      
        settingsMenu.SetActive(isInSettingsMenu);
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
        settings.SetStartIndex(level-1);
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

    public void LevelCollectionButton(int i) =>  CollectionLevel = i;
    #endregion
    
    #region UIMethods
    private event Action<int> OnGoldChangeValue;
    private event Action<int> OnStarChangeValue;
    private event Action<int> OnCollectionLevelChange;

    public void InitUIAction()
    {
        void InitUIGold(int i)
        {
            goldCountText.text = i.ToString();
        }
        void InitUIStar(int i)
        {
            starCountText.text = i.ToString();
        }
        void DebugCollectionLevel(int i)
        {
            Debug.Log($"Level Collection : {collectionLevel}");
        }        

        OnGoldChangeValue = InitUIGold;
        OnStarChangeValue = InitUIStar;
        OnCollectionLevelChange = DebugCollectionLevel;
    }
    #endregion
}

