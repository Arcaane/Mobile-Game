using UnityEditor;
using UnityEngine;
using System;
using NaughtyAttributes;

public class ScriptableForItemCollection : EditorWindow
{
    [MenuItem("Tool/ScriptableForItemCollection")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ScriptableForItemCollection));
    }
    
    public Sprite itemSprite;
    [ResizableTextArea] public string objectTitle;
    [ResizableTextArea] public string descriptionText;
    [ResizableTextArea] public string powerUpText;
    public int chapterNumber;
    public ItemRarety rarety;
    
    private string path = "Assets/Level Design/Scriptable Levels/";

    private void OnGUI()
    {
        GUILayout.Label("Item", EditorStyles.boldLabel);
        
        itemSprite = EditorGUILayout.ObjectField("NPC 1 Image", itemSprite, typeof(Sprite), false) as Sprite;
        objectTitle = EditorGUILayout.TextField("Item Title", objectTitle);
        descriptionText = EditorGUILayout.TextField("Item Description", descriptionText);
        powerUpText = EditorGUILayout.TextField("Item powerUp", powerUpText);
        chapterNumber = EditorGUILayout.IntField("Chapter Number", chapterNumber);
        rarety = (ItemRarety)EditorGUILayout.EnumPopup("Chapter for current item", rarety);
        
        GUI.enabled = objectTitle != String.Empty && descriptionText != String.Empty;
        if (GUILayout.Button("Create Scriptable"))
        {
            GenerateScriptable();
        }
        
        EditorGUILayout.Space(10);
        GUI.enabled = true;
        if (GUILayout.Button("Close Window")) Close();
    }

    private void GenerateScriptable()
    {
        var temp = ScriptableObject.CreateInstance<CollectionItem>();
        var assetPath = $"{path}ItemCollection_{objectTitle}_Item.asset";
        
        temp.itemSprite = this.itemSprite;
        temp.objectTitle = this.objectTitle;
        temp.descriptionText = this.descriptionText;
        temp.chapterNumber = $"Chapter {this.chapterNumber}";
        //temp.Rarity = $"Rarety : {Enum.GetName(typeof(ItemRarety), this.rarety)}";
        temp.powerUpText = this.powerUpText;
        
        AssetDatabase.CreateAsset(temp, assetPath);
        Debug.Log($"Asset {objectTitle}_Item.asset well created !");
    }
}

public enum ItemRarety
{
    Rare, Epic, Legendary
}
