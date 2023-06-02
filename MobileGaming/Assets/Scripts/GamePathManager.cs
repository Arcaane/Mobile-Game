using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GamePathManager : MonoBehaviour
{
    public LevelSelectionContentHolder levelSelection;
    public LevelPreScreenContentHolder preScreenLevel;

    public LevelDisplaySagaMap[] levels;
    public int unlockedLevels = 1;
    
    private void Start()
    {
        if (!PlayerPrefs.HasKey("LevelUnlocked")) PlayerPrefs.SetInt("LevelUnlocked", 1);
        unlockedLevels = PlayerPrefs.GetInt("LevelUnlocked");
        
        for (int i = 0; i < levels.Length; i++)
        {
            var level = levels[i];
            level.UnlockLevel(i < unlockedLevels);
        }

        for (int i = levels.Length - 1; i >= 0; i--)
        {
            var level = levels[i];
            if(level.LevelScriptable == null) continue;
            if (level.LevelScriptable.Completed && !level.LevelScriptable.Fake)
            {
                if(level.NextLevel != null) UnlockNext(level.NextLevel);
                break;
            }
        }

        void UnlockNext(LevelDisplaySagaMap levelDisplaySagaMap)
        {
            if(levelDisplaySagaMap.NextLevel == null) return;
            levelDisplaySagaMap.UnlockLevel(true);
            if(!levelDisplaySagaMap.LevelScriptable.Fake) return;
            UnlockNext(levelDisplaySagaMap.NextLevel);
        }
    }
    
    

    
    
    public void SaveLevel(int level)
    {
        if (levels[level] == null) return;
        //PlayerPrefs.SetInt(levels[level].name, levels[level].starsClaimedCount);
        PlayerPrefs.SetInt("LevelUnlocked", unlockedLevels);
    }

    public Transform tr;
    public GameObject prefab;

    [ContextMenu("GRZGRZG")]
    private void ReplaceWithPrefab()
    {
        var sagaLevels = levels;
        for (int i = 0; i < sagaLevels.Length-1; i++)
        {
            sagaLevels[i].nextLevel = sagaLevels[i + 1];
            EditorUtility.SetDirty(sagaLevels[i]);
        }
        
    }
}