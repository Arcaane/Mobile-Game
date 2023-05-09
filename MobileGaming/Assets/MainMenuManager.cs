using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Settings")]
    public Button settingsButton;
    public bool isInSettingsMenu;
    public GameObject settingsMenu;
    [SerializeField] private ScriptableSettings settings;
    
    public RectTransform levelPanelRectTransform;
    
    // Start is called before the first frame update
    void Start()
    {
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
        SceneManager.LoadScene(1);
    }
}

