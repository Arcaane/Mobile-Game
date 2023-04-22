using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LevelInitializer : MonoBehaviour
{
    public string levelStringKey;
    private char c;
    private int blockIndex;
    private float timerOfTheLevel;

    [Header("Params")] 
    
    public Vector2Int levelSize;
    [Space(10)] public BlockData[] blockData;
    
    public void Bake()
    {
        ClearGrid();
        string[] separatedStringKey = levelStringKey.Split(';');

        levelSize.x = int.Parse(separatedStringKey[0]);
        levelSize.y = int.Parse(separatedStringKey[1]);
        timerOfTheLevel = float.Parse(separatedStringKey[2]);
        
        for (int x = 0; x < levelSize.x; x++)
        {
            for (var y = 0; y < levelSize.y; y++)
            {
                //c = levelStringKey[blockIndex];
                c = separatedStringKey[3][blockIndex];

                var go = PrefabUtility.InstantiatePrefab(SetBlockByChar(c), transform) as GameObject;

                if (go != null) go.transform.SetPositionAndRotation(new Vector3(x, 0, y), Quaternion.identity);

                //GameObject GO = Instantiate(SetBlockByChar(c), new Vector3(x, 0, y), Quaternion.identity, transform);
                blockIndex++;
            }
        }
    }

    private GameObject SetBlockByChar(char actualChar)
    {
        return (from data in blockData where data.c == actualChar select data.caseObject).FirstOrDefault();
    }

    public Char SetCharByBlock(TypeCases actualType)
    {
        return (from data in blockData where data.type == actualType select data.c).FirstOrDefault();
    }

    public void ClearGrid()
    {
        blockIndex = 0;
        var childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}

public enum TypeCases
{
    Client, Floor, Empty, MachineAnchor, Obstacle, PowerUp, Wall 
}

[Serializable]
public struct BlockData
{
    public char c;
    public TypeCases type;
    public GameObject caseObject;
}
