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
    [SerializeField] private ScriptableSettings settings;

    public RectTransform levelPanelRectTransform;

    [Space(10)] [Header("User variables")] 
    [SerializeField] private TextMeshProUGUI starCountText;
    [SerializeField] private TextMeshProUGUI goldCountText;
    
    private int starCount;
    private int goldCount;
    
    [field:SerializeField] public int StarCount
    {
        get => starCount;
        set
        {
            starCount = value;
            OnStarChangeValue.Invoke(value);
        }
    }
    [SerializeField] public int GoldCount
    {
        get => goldCount;
        set
        {
            goldCount = value;
            OnGoldChangeValue.Invoke(value);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("Star")) PlayerPrefs.SetInt("Star", StarCount);
        if (!PlayerPrefs.HasKey("Gold")) PlayerPrefs.SetInt("Gold", GoldCount);
        
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
    #endregion


    #region UIMethods
    private Action<int> OnGoldChangeValue;
    private Action<int> OnStarChangeValue;

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

        OnGoldChangeValue = InitUIGold;
        OnStarChangeValue = InitUIStar;
    }
    #endregion
}

