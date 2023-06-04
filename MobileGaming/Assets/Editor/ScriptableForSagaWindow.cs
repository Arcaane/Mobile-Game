using System;
using UnityEditor;
using UnityEngine;
using NaughtyAttributes;

public class ScriptableForSagaWindow : EditorWindow
{
    [MenuItem("Tool/Scriptable Generator Level In Saga")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ScriptableForSagaWindow));
    }
    
    private string title;
    [ResizableTextArea] private string areaText1; 
    [ResizableTextArea] private string areaText2;
    
    private int currentLevel;
    private int levelObjective;
    private int gearCountPlayerCanEquip;
    
    private Sprite fragementReward;
    private Sprite potionToUseSprite;

    private string npcName1;
    private Sprite npcImage1;
    private int ncpScore1;
    
    private string npcName2;
    private Sprite npcImage2;
    private int ncpScore2;

    private string path = "Assets/Level Design/Scriptable Levels/";

    Vector2 scrollPosition = Vector2.zero;
    private void OnGUI()
    {
        GUILayout.BeginScrollView(scrollPosition, false, false); 
        
        // COMMON PART
        GUILayout.Label("Common UI", EditorStyles.boldLabel);
        title = EditorGUILayout.TextField("Level Title", title);
        
        // Level Selection Part
        EditorGUILayout.Space(20);
        GUILayout.Label("Level Selection", EditorStyles.boldLabel);
        currentLevel = EditorGUILayout.IntField("Current Level", currentLevel);
        levelObjective = EditorGUILayout.IntField("Level Objective", levelObjective);
        //levelSelectionBackground = EditorGUILayout.ObjectField("LevelSelectionBackground", levelSelectionBackground, typeof(Sprite), false) as Sprite;
        gearCountPlayerCanEquip = EditorGUILayout.IntField("Gear Count Unlocked", gearCountPlayerCanEquip);
        EditorGUILayout.Space(15);
        GUILayout.Label("Social", EditorStyles.boldLabel);
        GUILayout.Label("NPC 1", EditorStyles.boldLabel);
        npcName1 = EditorGUILayout.TextField("NPC 1 Name", npcName1);
        npcImage1 = EditorGUILayout.ObjectField("NPC 1 Image", npcImage1, typeof(Sprite), false) as Sprite;
        ncpScore1 = EditorGUILayout.IntField("NPC 1 Score", ncpScore1);
        EditorGUILayout.Space(7.5f);
        GUILayout.Label("NPC 2", EditorStyles.boldLabel);
        npcName2 = EditorGUILayout.TextField("NPC 2 Name", npcName2);
        npcImage2 = EditorGUILayout.ObjectField("NPC 2 SPRITE", npcImage2, typeof(Sprite), false) as Sprite;
        ncpScore2 = EditorGUILayout.IntField("NPC 2 Score", ncpScore2);
        EditorGUILayout.Space(15);
        GUILayout.Label("Pre-Game Screen", EditorStyles.boldLabel);
        areaText1 = EditorGUILayout.TextField("Area Text 1", areaText1);
        areaText2 = EditorGUILayout.TextField("Area Text 2", areaText2);
        //preScreenLevelBackground = EditorGUILayout.ObjectField("Pre-Screen Level Background Sprite", preScreenLevelBackground, typeof(Sprite), false) as Sprite;
        fragementReward = EditorGUILayout.ObjectField("Fragement Reward Sprite", fragementReward, typeof(Sprite), false) as Sprite;
        potionToUseSprite = EditorGUILayout.ObjectField("Advise Potion  Sprite", potionToUseSprite, typeof(Sprite), false) as Sprite;
        
        GUI.enabled = title != String.Empty && areaText1 != String.Empty && areaText2 != String.Empty && currentLevel != 0 && levelObjective != 0;
        if (GUILayout.Button("Create Scriptable"))
        {
            GenerateScriptable();
        }
        
        EditorGUILayout.Space(10);
        GUI.enabled = true;
        if (GUILayout.Button("Close Window")) Close();
        
        GUILayout.EndScrollView();
    }

    private void GenerateScriptable()
    {
        ScriptableLevelInSagaMap temp = ScriptableObject.CreateInstance<ScriptableLevelInSagaMap>();
        var assetPath = $"{path}Level_{currentLevel}_LevelAsset.asset";
        
        temp.title = title;
        temp.areaText1 = areaText1;
        temp.areaText2 = areaText2;
        //temp.currentLevel = currentLevel;
        //temp.levelObjective = levelObjective;
        temp.gearCountPlayerCanEquip = gearCountPlayerCanEquip;
        temp.fragementReward = fragementReward;
        temp.potionToUseSprite = potionToUseSprite;
        
        temp.socialInfos = new[]
        {
            new SocialInfo(null, null, 0), 
            new SocialInfo(npcName1, npcImage1, ncpScore1), 
            new SocialInfo(npcName2, npcImage2, ncpScore2)
        };
        
        AssetDatabase.CreateAsset(temp, assetPath);
        Debug.Log($"Asset {currentLevel}_LevelAsset.asset well created !");
    }
}
