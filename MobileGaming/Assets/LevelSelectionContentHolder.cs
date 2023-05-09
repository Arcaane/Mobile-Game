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

    

    public void BuildUI(ScriptableObject _scriptableObject)
    {
        
    }
}

[Serializable]
public struct SocialInfo
{
    public Image image;
    public TextMeshProUGUI name;
    public TextMeshProUGUI score;
}
