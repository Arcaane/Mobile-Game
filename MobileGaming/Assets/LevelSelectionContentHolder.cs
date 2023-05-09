using System;
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
    public Image sectionBackground;
    public Image[] starsImage;
    public Image[] gearImage;
    
    [Header("Social Info Section")] [Space(5)]
    public SocialInfo[] players;
    
    /*public Image playerImage;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerScore;
    [Space(5)]
    public Image npc1Image;
    public TextMeshProUGUI npc1Name;
    public TextMeshProUGUI npc1Score;
    [Space(5)]
    public Image npc2Image;
    public TextMeshProUGUI npc2Name;
    public TextMeshProUGUI npc2Score;*/
    
    public void BuildUI(ScriptableLevelInSagaMap _scriptableObject)
    {
        levelTitleText.text = _scriptableObject.title;
        currentLevelText.text = $"Level: {_scriptableObject.currentLevel}";
        levelObjective.text = $"Objective: {_scriptableObject.levelObjective}";
        sectionBackground.sprite = _scriptableObject.levelSelectionBackground;
        
        // Voir pour les stars
        // Voir pour le gear

        for (int i = 0; i < _scriptableObject.socialInfos.Length; i++)
        {
            players[i].image = _scriptableObject.socialInfos[i].image;
            players[i].name = _scriptableObject.socialInfos[i].name;
            players[i].score = _scriptableObject.socialInfos[i].score;
        }
    }
}

[Serializable]
public struct SocialInfo
{
    public Image image;
    public TextMeshProUGUI name;
    public TextMeshProUGUI score;
}
