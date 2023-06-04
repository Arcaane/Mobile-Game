using System;
using Service;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionContentHolder : MonoBehaviour
{
    [Header("Level Info Section")]
    // Text
    public TextMeshProUGUI levelTitleText;
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI levelObjective;
    
    // Images
    [Header("Components")]
    [SerializeField] private GameObject panelGo;
    public Image sectionBackground;
    public Image[] starsImage;
    public Image[] gearImage;
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    
    [Header("Social Info Section")] [Space(5)]
    public SocialInfoHolder[] socialHolder;

    private ScriptableLevelInSagaMap scriptableLevelInSagaMap;

    public void Start()
    {
        exitButton.onClick.AddListener(HidePanel);
        startButton.onClick.AddListener(StartLevel);
        
        
        void HidePanel()
        {
            panelGo.SetActive(false);
        }

        void StartLevel()
        {
            GameService.LoadLevel(scriptableLevelInSagaMap.LevelScene);
        }
    }

    public void OnEnable()
    {
        EventManager.AddListener<OpenLevelSagaMapEvent>(BuildUI);
    }

    public void OnDisable()
    {
        EventManager.RemoveListener<OpenLevelSagaMapEvent>(BuildUI);
    }

    private void BuildUI(OpenLevelSagaMapEvent openLevelSagaMapEvent)
    {
        panelGo.SetActive(true);
        scriptableLevelInSagaMap = openLevelSagaMapEvent.ScriptableLevelInSagaMap;
        if (scriptableLevelInSagaMap == null)
        {
            panelGo.SetActive(false);
            return;
        }
        
        if(levelTitleText != null) levelTitleText.text = scriptableLevelInSagaMap.title;
        currentLevelText.text = $"Level: {scriptableLevelInSagaMap.CurrentLevel}";
        levelObjective.text = $"Objective: {scriptableLevelInSagaMap.ScoreToWin}";
        sectionBackground.sprite = scriptableLevelInSagaMap.levelSelectionBackground;
        
        // TODO - Voir pour les stars
        for (int i = 0; i < starsImage.Length; i++)
        {
            starsImage[i].gameObject.SetActive(i < openLevelSagaMapEvent.ScriptableLevelInSagaMap.Stars);
        }
    }
}

[Serializable]
public struct SocialInfo
{
    public string name;
    public Sprite image;
    public int score;

    public SocialInfo(string npcName1, Sprite imgTemp, int ncpScore1)
    {
        name = npcName1;
        image = imgTemp;
        score = ncpScore1;
    }
}

[Serializable]
public class SocialInfoHolder
{
    public TextMeshProUGUI name;
    public Image image;
    public TextMeshProUGUI score;
}
