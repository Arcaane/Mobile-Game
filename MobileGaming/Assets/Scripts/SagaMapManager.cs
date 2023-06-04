using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SagaMapManager : MonoBehaviour
{
    public List<LevelDisplaySagaMap> levels;
    private List<ScriptableLevelInSagaMap> scriptableLevelsInSagaMap;
    [SerializeField] private Transform sagaMapPanelTr;
    private RectTransform sagaMapPanelRectTr;
    public int unlockedLevels = 1;

    private void OnEnable()
    {
        EventManager.AddListener<ResetPlayerPrefsEvent>(ResetLevelProgress);
        EventManager.AddListener<ResetLevelsEvent>(ResetLevels);
        EventManager.AddListener<UpdateLevelsEvent>(UpdateLevels);
        EventManager.AddListener<ResetPlayerPrefsEvent>(GetProgress);
        GetProgress();
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<ResetPlayerPrefsEvent>(ResetLevelProgress);
        EventManager.RemoveListener<ResetLevelsEvent>(ResetLevels);
        EventManager.RemoveListener<UpdateLevelsEvent>(UpdateLevels);
        EventManager.RemoveListener<ResetPlayerPrefsEvent>(GetProgress);
    }

    private void UpdateLevels(UpdateLevelsEvent _)
    {
        GetProgress();
    }

    private void ResetLevels(ResetLevelsEvent _)
    {
        ResetLevelProgress(null);
    }

    private void ResetLevelProgress(ResetPlayerPrefsEvent _)
    {
        scriptableLevelsInSagaMap = levels.Where(level => level.LevelScriptable != null).Select(level => level.LevelScriptable)
            .OrderBy(scriptable => scriptable.CurrentLevel).ToList();
        
        foreach (var scriptableLevelInSaga in scriptableLevelsInSagaMap)
        {
            scriptableLevelInSaga.ResetProgress();
        }
        
        EventManager.Trigger(new UpdateLevelsEvent());
    }
    
    private void GetProgress(ResetPlayerPrefsEvent _ = null)
    {
        scriptableLevelsInSagaMap = levels.Where(level => level.LevelScriptable != null).Select(level => level.LevelScriptable)
            .OrderBy(scriptable => scriptable.CurrentLevel).ToList();
        
        if (!PlayerPrefs.HasKey("LevelUnlocked")) PlayerPrefs.SetInt("LevelUnlocked", 1);
        unlockedLevels = PlayerPrefs.GetInt("LevelUnlocked");
        if (unlockedLevels <= 0)
        {
            PlayerPrefs.SetInt("LevelUnlocked", 1);
            unlockedLevels = 1;
        }
        
        for (int i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            level.UnlockLevel(i < unlockedLevels);
            //Debug.Log($"level {level.gameObject} y : {sagaMapPanelTr.InverseTransformPoint(level.transform.localPosition).y}",level);
        }

        var lastUnlockedLevel = levels[0].LevelScriptable;
        for (int i = levels.Count - 1; i >= 0; i--)
        {
            var level = levels[i];
            if(level.LevelScriptable == null) continue;
            if (!level.LevelScriptable.Completed || level.LevelScriptable.Fake) continue;
            if(level.NextLevel != null) UnlockNext(level.NextLevel);
            break;
        }

        sagaMapPanelRectTr = sagaMapPanelTr.GetComponent<RectTransform>();
        var pos = sagaMapPanelRectTr.anchoredPosition;
        pos.y = lastUnlockedLevel.autoscrollPositionY;
        sagaMapPanelRectTr.anchoredPosition = pos;
        
        void UnlockNext(LevelDisplaySagaMap levelDisplaySagaMap)
        {
            if(levelDisplaySagaMap.NextLevel == null) return;
            levelDisplaySagaMap.UnlockLevel(true);
            lastUnlockedLevel = levelDisplaySagaMap.LevelScriptable;
            if(!levelDisplaySagaMap.LevelScriptable.Fake) return;
            UnlockNext(levelDisplaySagaMap.NextLevel);
        }
        
        EventManager.Trigger(new RefreshSagaMapLevelsEvent(scriptableLevelsInSagaMap));
    }

    public float amount;
    
    [ContextMenu("Move")]
    public void MoveSagaMap()
    {
        var position = sagaMapPanelRectTr.localPosition;
        position.y = - amount - Screen.height/2f;
        sagaMapPanelRectTr.localPosition = position;
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

public class UpdateLevelsEvent { }