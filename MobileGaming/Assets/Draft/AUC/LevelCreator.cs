using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Grid))]
public class LevelCreator : MonoBehaviour
{
   [Header("References PAS TOUCHE")]
   [SerializeField] private LevelInitializer levelInitializer;
   [SerializeField] private Grid grid;
   
   [Space]
   [Header("Key Gen")] 
   [Tooltip("Clé du niveau")]
   [SerializeField] private string outputLevelKey;
   
   [Space]
   [Header("LevelEditorSection")] 
   [Tooltip("Taille du LD au format 0x0 ")]
   public Vector2Int levelSize;
   
   [Tooltip("Temps que le joueur a pour finir le niveau")]
   public float levelTimer = default;
   
   [Tooltip("Score que le joueur doit atteindre pour finir le niveau")]
   public float scoreForEndLevel = default;
   
   [Space]
   [Header("Client List")]
   [Tooltip("Liste des clients présent dans le niveau")]
   public List<ClientSlot> clientList;
   
   [Space]
   [Header("Client List")]
   [Tooltip("Liste des machine présentes dans le niveau")]
   public List<Machine> machineList;
   
    
    // Private region
    private List<Tile> childList = new();
    private List<Tile> sortedList = new();
    
   public void InitGrid()
   {
      outputLevelKey = String.Empty;
      ClearGrid();

      for (int i = 0; i < levelSize.x; i++)
      {
         for (int j = 0; j < levelSize.y; j++)
         {
            Instantiate(levelInitializer.blockData[0].caseObject, new Vector3(i +.5f, -.5f, j+.5f), Quaternion.identity, transform);
         }
      }
   }

   public void ClearGrid()
   {
      var childCount = transform.childCount;
      for (int i = 0; i < childCount; i++)
      {
         DestroyImmediate(transform.GetChild(0).gameObject);
      }
   }

   public void GenerateKey()
   {
      outputLevelKey = String.Empty;
      outputLevelKey += $"{levelSize.x};{levelSize.y};{levelTimer};";
      
      GetAllTiles();
      SortTile();

      for (int i = 0; i < childList.Count; i++)
      {
         outputLevelKey += levelInitializer.SetCharByBlock(sortedList[i].typeCases);
      }
      
      levelInitializer.levelStringKey = outputLevelKey;
      levelInitializer.Bake();
   }

   private void GetAllTiles()
   {
      childList.Clear();
      for (int i = 0; i < transform.childCount; i++)
      {
         Tile tempTile = transform.GetChild(i).GetComponent<Tile>();
         childList.Add(tempTile);

         var position = tempTile.transform.position;
         tempTile.posOnTileSet = new Vector3Int((int)((int)position.x + 0.5f), 0, (int)((int)position.z + 0.5f));
      }
   }

   private void SortTile()
   {
      sortedList.Clear();
      sortedList = childList.ToList();
      sortedList.Sort();
   }
   public void BuildLevel()
   {
      GenerateKey();
      //levelInitializer.levelSize = levelSize;
      levelInitializer.levelStringKey = outputLevelKey;
      levelInitializer.Bake();
   }
}
