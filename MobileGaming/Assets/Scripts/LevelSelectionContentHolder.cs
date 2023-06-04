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
        //Debug.Log($"Building ui for {scriptableLevelInSagaMap}, going to {scriptableLevelInSagaMap.LevelScene}");
        if (scriptableLevelInSagaMap == null)
        {
            panelGo.SetActive(false);
            return;
        }
        
        if(levelTitleText != null) levelTitleText.text = scriptableLevelInSagaMap.title;
        currentLevelText.text = $"Level: {scriptableLevelInSagaMap.CurrentLevel}";
        levelObjective.text = $"Objective: {scriptableLevelInSagaMap.ScoreToWin}";
        //sectionBackground.sprite = _scriptableObject.levelSelectionBackground;
        
        // TODO - Voir pour les stars
        // TODO - Voir pour le gear
        // TODO - Partie Social Player

        for (int i = 1; i < scriptableLevelInSagaMap.socialInfos.Length; i++)
        {
            socialHolder[i].image.sprite = scriptableLevelInSagaMap.socialInfos[i].image;
            socialHolder[i].name.text = scriptableLevelInSagaMap.socialInfos[i].name;
            socialHolder[i].score.text = $"Score: {scriptableLevelInSagaMap.socialInfos[i].score}";
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
