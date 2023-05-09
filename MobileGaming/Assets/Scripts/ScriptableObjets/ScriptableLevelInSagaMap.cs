using NaughtyAttributes;
using UnityEngine;

[AddComponentMenu("LevelInSagaMap")]
public class ScriptableLevelInSagaMap : ScriptableObject
{
    [Header("Common to two parts")]
    public string title = $"Level title";
    public int currentLevel = 1;

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
}
