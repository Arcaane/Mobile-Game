using NaughtyAttributes;
using UnityEngine;

[AddComponentMenu("LevelInSagaMap")]
public class ScriptableLevelInSagaMap : ScriptableObject
{
    [Header("Common to two parts")]
    public string title = $"Level title";
    [field: SerializeField] public int CurrentChapter { get; private set; }
    [field: SerializeField] public int CurrentLevel { get; private set; }
    [field: SerializeField] public bool IsLastLevelOfChapter { get; private set; }
    [field: SerializeField] public bool Fake { get; private set; }
    [field: SerializeField,Scene] public int LevelScene { get; private set; }
    [field: SerializeField,Scene] public int NextLevelScene { get; private set; }
    
    [Space(5)] [Header("Level Selection Section")]
    public int levelObjective;
    public int gearCountPlayerCanEquip;
    //[ShowAssetPreview] public Sprite levelSelectionBackground;

    // Social
    public SocialInfo[] socialInfos;

    [Space(20)] [Header("Pre-Screen Level Section")]
    //[ShowAssetPreview] public Sprite preScreenLevelBackground;
    [ShowAssetPreview] public Sprite fragementReward;
    [ShowAssetPreview] public Sprite potionToUseSprite;
    [ResizableTextArea] public string areaText1;
    [ResizableTextArea] public string areaText2;

    //Save
    public bool Completed => Stars > 0 || Fake;
    public int Score { get; private set; }
    public int Stars { get; private set; }

    public void GetProgress()
    {
        if (!PlayerPrefs.HasKey($"{name}_Score")) PlayerPrefs.SetInt($"{name}_Score", 0);
        Score = PlayerPrefs.GetInt($"{name}_Score");
        
        if (!PlayerPrefs.HasKey($"{name}_Stars")) PlayerPrefs.SetInt($"{name}_Stars", 0);
        Stars = PlayerPrefs.GetInt($"{name}_Stars");
    }

    public void SetProgress(int newStars,int newScore)
    {
        var increaseStars = newStars - Stars;
        if (increaseStars > 0)
        {
            Stars = newStars;
            PlayerPrefs.SetInt($"{name}_Stars",Stars);
            EventManager.Trigger(new GainStarEvent(increaseStars));
        }


        var increaseScore = newScore - Score;
        if (increaseScore > 0)
        {
            Score = newScore;
            PlayerPrefs.SetInt($"{name}_Score",Score);
            EventManager.Trigger(new GainScoreEvent(increaseScore));
        }
    }
}

public class GainStarEvent
{
    public int Amount { get; }

    public GainStarEvent(int amount)
    {
        Amount = amount;
    }
}

public class GainScoreEvent
{
    public int Amount { get; }

    public GainScoreEvent(int amount)
    {
        Amount = amount;
    }
}



