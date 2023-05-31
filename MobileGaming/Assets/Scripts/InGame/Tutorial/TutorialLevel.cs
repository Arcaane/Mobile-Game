using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevel : Level
{
    [SerializeField] private TutorialCanvas tutorialCanvas;
    private RectTransform cursorTr;
    
    private void Start()
    {
        Debug.Log("Starting tutorial");
        
        cursorTr = tutorialCanvas.CursorTr;
        
        tutorialCanvas.ShowCursor(false);
        
        EventManager.AddListener<StartLevelEvent>(StartTutorial);
        
        EventManager.Trigger(new LoadLevelEvent(this));
        EventManager.Trigger(new LoadTutorialEvent(this));
    }

    private void StartTutorial(StartLevelEvent startLevelEvent)
    {
        EventManager.RemoveListener<StartLevelEvent>(StartTutorial);

        
        tutorialCanvas.ShowCursor(true);
        tutorialCanvas.PlayFirstSequence();
    }
}

public class LoadTutorialEvent
{
    public TutorialLevel Level { get; }

    public LoadTutorialEvent(TutorialLevel tutorialLevel)
    {
        Level = tutorialLevel;
    }
}
