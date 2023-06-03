using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SagaMapManager : MonoBehaviour
{
    public List<LevelDisplaySagaMap> levels;
    private List<ScriptableLevelInSagaMap> scriptableLevelsInSagaMap;
    [SerializeField] private Transform sagaMapPanelTr;
    public int unlockedLevels = 1;
    
    private void Start()
    {
        scriptableLevelsInSagaMap = levels.Where(level => level.LevelScriptable != null).Select(level => level.LevelScriptable)
            .OrderBy(scriptable => scriptable.CurrentLevel).ToList();
        
        if (!PlayerPrefs.HasKey("LevelUnlocked")) PlayerPrefs.SetInt("LevelUnlocked", 1);
        unlockedLevels = PlayerPrefs.GetInt("LevelUnlocked");
        if (unlockedLevels < 0)
        {
            PlayerPrefs.SetInt("LevelUnlocked", 1);
            unlockedLevels = 1;
        }
        
        for (int i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            level.UnlockLevel(i < unlockedLevels);
        }

        var lastUnlockedLevel = levels[0];
        for (int i = levels.Count - 1; i >= 0; i--)
        {
            var level = levels[i];
            if(level.LevelScriptable == null) continue;
            if (!level.LevelScriptable.Completed || level.LevelScriptable.Fake) continue;
            if(level.NextLevel != null) UnlockNext(level.NextLevel);
            break;
        }
        Debug.Log($"Last unlocked level : {lastUnlockedLevel}");

        void UnlockNext(LevelDisplaySagaMap levelDisplaySagaMap)
        {
            if(levelDisplaySagaMap.NextLevel == null) return;
            levelDisplaySagaMap.UnlockLevel(true);
            lastUnlockedLevel = levelDisplaySagaMap;
            if(!levelDisplaySagaMap.LevelScriptable.Fake) return;
            UnlockNext(levelDisplaySagaMap.NextLevel);
        }
        
        EventManager.Trigger(new RefreshSagaMapLevelsEvent(scriptableLevelsInSagaMap));
    }
}

public class RefreshSagaMapLevelsEvent
{
    public List<ScriptableLevelInSagaMap> ScriptableLevelsInSagaMap { get; }

    public RefreshSagaMapLevelsEvent(List<ScriptableLevelInSagaMap> scriptableLevelInSagaMaps)
    {
        ScriptableLevelsInSagaMap = scriptableLevelInSagaMaps;
    }
}