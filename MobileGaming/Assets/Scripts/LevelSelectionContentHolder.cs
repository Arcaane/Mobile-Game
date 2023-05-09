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
    public SocialInfoHolder[] socialHolder;
    
    public void BuildUI(ScriptableLevelInSagaMap _scriptableObject)
    {
        levelTitleText.text = _scriptableObject.title;
        currentLevelText.text = $"Level: {_scriptableObject.currentLevel}";
        levelObjective.text = $"Objective: {_scriptableObject.levelObjective}";
        //sectionBackground.sprite = _scriptableObject.levelSelectionBackground;
        
        // TODO - Voir pour les stars
        // TODO - Voir pour le gear
        // TODO - Partie Social Player

        /*for (int i = 1; i < _scriptableObject.socialInfos.Length; i++)
        {
            socialHolder[i].image.sprite = _scriptableObject.socialInfos[i].image;
            socialHolder[i].name.text = _scriptableObject.socialInfos[i].name;
            socialHolder[i].score.text = $"Score: {_scriptableObject.socialInfos[i].score}";
        }*/
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
