using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevel : Level
{
    [SerializeField] private TutorialCanvas tutorialCanvas;

    private List<ILinkable> expectedStartLinkables = new List<ILinkable>();
    private List<ILinkable> expectedEndLinkables = new List<ILinkable>();

    private Queue<Action> sequenceQueue = new Queue<Action>();

    private void Start()
    {
        Debug.Log("Starting tutorial");
        
        levelDuration = 2f;
        
        
        sequenceQueue.Clear();
        tutorialCanvas.ShowCursor(false);
        
        EventManager.AddListener<StartLevelEvent>(StartTutorial);
        EventManager.AddListener<EndLevelEvent>(RemoveTutorialModifications);
        
        EventManager.AddListener<LinkCreatedEvent>(DestroyIfInvalidLink);
        EventManager.AddListener<LinkCreatedEvent>(DestroyIfGeneratorToClientLink);
        EventManager.AddListener<LinkDestroyedEvent>(GoToNextStepOnCompleteLink);
        
        EventManager.AddListener<LevelTimeUpdatedEvent>(DelayTimer);

        EventManager.Trigger(new LoadLevelEvent(this));
        EventManager.Trigger(new LoadTutorialEvent());
    }
    
    private void StartTutorial(StartLevelEvent startLevelEvent)
    {
        EventManager.RemoveListener<StartLevelEvent>(StartTutorial);
        
        sequenceQueue.Enqueue(PlayFirstSequence);
        
        NextSequence();
    }

    private void RemoveTutorialModifications(EndLevelEvent _)
    {
        EventManager.RemoveListener<LinkCreatedEvent>(DestroyIfInvalidLink);
        EventManager.RemoveListener<LevelTimeUpdatedEvent>(DelayTimer);
        
        tutorialCanvas.StopSequence();
    }
    
    private void DestroyIfInvalidLink(LinkCreatedEvent linkCreatedEvent)
    {
        var link = linkCreatedEvent.Link;
        
        if (expectedStartLinkables.Contains(link.StartLinkable) && expectedEndLinkables.Contains(link.EndLinkable)) return;
        
        link.DestroyLink();
    }
    
    private void DestroyIfGeneratorToClientLink(LinkCreatedEvent linkCreatedEvent)
    {
        var link = linkCreatedEvent.Link;
        
        if(link.StartLinkable is not Machine machine) return;
        if(link.EndLinkable is not ClientSlot client) return;
        
        if(machine == machines[0] && client == clientSlots[0]) link.DestroyLink();
    }

    private void GoToNextStepOnCompleteLink(LinkDestroyedEvent linkDestroyedEvent)
    {
        if(!linkDestroyedEvent.CompletedTransfer) return;

        var link = linkDestroyedEvent.Link;
        expectedStartLinkables.Remove(link.StartLinkable);
        expectedEndLinkables.Remove(link.EndLinkable);
        if(expectedStartLinkables.Count <= 0 && expectedEndLinkables.Count <= 0) NextSequence();
    }
    
    private void DelayTimer(LevelTimeUpdatedEvent levelTimeUpdatedEvent)
    {
        levelTimeUpdatedEvent.Service.IncreaseLevelDuration(Time.deltaTime);
    }
    
    private void NextSequence()
    {
        if(sequenceQueue.Count <= 0) return;
        
        sequenceQueue.Dequeue().Invoke();
    }

    private void PlayFirstSequence()
    {
        expectedStartLinkables.Clear();
        expectedEndLinkables.Clear();
        
        expectedStartLinkables.Add(machines[0]);
        expectedEndLinkables.Add(machines[1]);
        
        tutorialCanvas.PlayFirstSequence();
        
        sequenceQueue.Enqueue(PlaySecondSequence);
    }

    private void PlaySecondSequence()
    {
        expectedStartLinkables.Clear();
        expectedEndLinkables.Clear();
        
        expectedStartLinkables.Add(machines[1]);
        expectedEndLinkables.Add(clientSlots[0]);
        
        tutorialCanvas.PlaySecondSequence();
        
        sequenceQueue.Enqueue(PlayThirdSequence);
    }
    
    private void PlayThirdSequence()
    {
        expectedStartLinkables.Clear();
        expectedEndLinkables.Clear();
        
        expectedStartLinkables.Add(machines[0]);
        expectedStartLinkables.Add(machines[2]);
        expectedEndLinkables.Add(machines[2]);
        expectedEndLinkables.Add(clientSlots[0]);
        
        tutorialCanvas.PlayThirdSequence();
        
        sequenceQueue.Enqueue(PlayFourthSequence);
    }
    
    private void PlayFourthSequence()
    {
        expectedStartLinkables.Clear();
        expectedEndLinkables.Clear();
        
        
        
        tutorialCanvas.PlayFourthSequence();
        
        sequenceQueue.Enqueue(EndTutorial);
    }

    private void EndTutorial()
    {
        EventManager.RemoveListener<LevelTimeUpdatedEvent>(DelayTimer);
    }
}

public class LoadTutorialEvent
{
    
}
