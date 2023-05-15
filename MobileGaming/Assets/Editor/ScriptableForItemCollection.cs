using UnityEditor;
using UnityEngine;
using System;

public class ScriptableForItemCollection : EditorWindow
{
    [MenuItem("Tool/ScriptableForItemCollection")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ScriptableForItemCollection));
    }
    
    public Sprite itemSprite;
    public string objectTitle;
    public string descriptionText;
    public int chapterNumber;
    public ItemRarety rarety;
    
    private string path = "Assets/Level Design/Scriptable Levels/";

    private void OnGUI()
    {
        GUILayout.Label("Item", EditorStyles.boldLabel);
        
        itemSprite = EditorGUILayout.ObjectField("NPC 1 Image", itemSprite, typeof(Sprite), false) as Sprite;
        objectTitle = EditorGUILayout.TextField("Item Title", objectTitle);
        descriptionText = EditorGUILayout.TextField("Item Description", descriptionText);
        chapterNumber = EditorGUILayout.IntField("Chapter Number", chapterNumber);
        rarety = (ItemRarety)EditorGUILayout.EnumPopup("Chapter for current item", rarety);
        
        GUI.enabled = itemSprite != null && objectTitle != String.Empty && descriptionText != String.Empty && chapterNumber != 0;
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
        temp.rarety = $"Rarety : {Enum.GetName(typeof(ItemRarety), this.rarety)}";
        
        AssetDatabase.CreateAsset(temp, assetPath);
        Debug.Log($"Asset {objectTitle}_Item.asset well created !");
    }
}

public enum ItemRarety
{
    Rare, Epic, Legendary
}
