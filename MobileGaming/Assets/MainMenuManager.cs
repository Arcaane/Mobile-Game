using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Settings")]
    public Button settingsButton;
    public bool isInSettingsMenu;
    public GameObject settingsMenu;
    [SerializeField] private ScriptableSettings settings;
    
    [Space(5)]
    [Header("GamePath")]
    public Button gamePathButton;
    public bool isInGamePathMenu;
    public GameObject gamePathMenu;

    public RectTransform levelPanelRectTransform;
    
    // Start is called before the first frame update
    void Start()
    {
        settingsMenu.SetActive(isInSettingsMenu);   
        gamePathMenu.SetActive(isInGamePathMenu);   
    }
    
    public void ToggleSettings()
    {
        isInSettingsMenu = !isInSettingsMenu;
        settingsMenu.SetActive(isInSettingsMenu);
    }

    public void ToggleGamePath()
    {
        isInGamePathMenu = !isInGamePathMenu;
        gamePathMenu.SetActive(isInGamePathMenu);
        if (isInSettingsMenu) ToggleSettings();
    }

    public void PlaceHolderGoToGameScene(int level)
    {
        settings.SetStartIndex(level-1);
        SceneManager.LoadScene(1);
    }
}

