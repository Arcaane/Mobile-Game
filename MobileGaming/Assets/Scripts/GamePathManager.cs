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
}