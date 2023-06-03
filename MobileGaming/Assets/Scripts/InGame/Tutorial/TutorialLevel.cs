using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevel : Level
{
    [SerializeField] private TutorialCanvas tutorialCanvas;
    [SerializeField] private LineRenderer tutorialLine;
    [SerializeField] private CollectionItem unlockedItem;

    private List<ILinkable> expectedStartLinkables = new List<ILinkable>();
    private List<ILinkable> expectedEndLinkables = new List<ILinkable>();
    private int minimumLinks = 1;

    private Queue<Action> sequenceQueue = new Queue<Action>();
    private IMagicLineService magicLineService;

    private void Start()
    {
        Debug.Log("Starting tutorial");
        
        levelDuration = 2f;
        tutorialLine.positionCount = 0;
        
        sequenceQueue.Clear();
        tutorialCanvas.ShowCursor(false);
        
        EventManager.Trigger(new LoadLevelEvent(this));
        
        EventManager.AddListener<StartLevelEvent>(StartTutorial);
        EventManager.AddListener<EndLevelEvent>(RemoveTutorialModifications);
        EventManager.AddListener<LinkCreatedEvent>(DestroyIfInvalidLink);
        EventManager.AddListener<LinkCreatedEvent>(DestroyIfGeneratorToClientLink);

        EventManager.AddListener<LevelTimeUpdatedEvent>(DelayTimer);

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
        EventManager.RemoveListener<EndLevelEvent>(RemoveTutorialModifications);
        EventManager.RemoveListener<LinkCreatedEvent>(DestroyIfInvalidLink);
        EventManager.RemoveListener<LinkCreatedEvent>(DestroyIfGeneratorToClientLink);
        EventManager.RemoveListener<LevelTimeUpdatedEvent>(DelayTimer);
        EventManager.RemoveListener<LinkDestroyedEvent>(NoLineDuringWork);
        sequenceQueue.Clear();
        NextSequence();
        
        tutorialCanvas.StopSequence();
    }
    
    private void DestroyIfInvalidLink(LinkCreatedEvent linkCreatedEvent)
    {
        var link = linkCreatedEvent.Link;

        if (linkCreatedEvent.TotalLinksCreated < minimumLinks)
        {
            link.DestroyLink();
            return;
        }
        
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

    private void NoLineDuringWork(LinkDestroyedEvent linkDestroyedEvent)
    {
        if(!linkDestroyedEvent.CompletedTransfer) return;
        expectedStartLinkables.Clear();
        expectedEndLinkables.Clear();
    }
    
    private void GoToNextStepOnWorkComplete(MachineEndWorkEvent machineEndWorkEvent)
    {
        NextSequence();
    }

    private void GoToNextStepOnCompleteLink(LinkDestroyedEvent linkDestroyedEvent)
    {
        if(!linkDestroyedEvent.CompletedTransfer) return;

        var link = linkDestroyedEvent.Link;
        expectedStartLinkables.Remove(link.StartLinkable);
        expectedEndLinkables.Remove(link.EndLinkable);
        if(expectedStartLinkables.Count <= 0 && expectedEndLinkables.Count <= 0) NextSequence();
    }

    private void GoToNextStepOnLinkDestroyed(LinkDestroyedEvent linkDestroyedEvent)
    {
        var link = linkDestroyedEvent.Link;
        if(link.StartLinkable is not Machine startMachine) return;
        if(link.EndLinkable is not Machine endMachine) return;
        if(startMachine != machines[2] || endMachine != machines[1]) return;
        NextSequence();
    }

    private void DelayTimer(LevelTimeUpdatedEvent levelTimeUpdatedEvent)
    {
        levelTimeUpdatedEvent.Service.IncreaseLevelDuration(Time.deltaTime);
        magicLineService ??= levelTimeUpdatedEvent.Service.MagicLineService;
    }
    
    private void NextSequence()
    {
        EventManager.RemoveListener<MachineEndWorkEvent>(GoToNextStepOnWorkComplete);
        EventManager.RemoveListener<LinkDestroyedEvent>(GoToNextStepOnCompleteLink);
        EventManager.RemoveListener<LinkDestroyedEvent>(GoToNextStepOnLinkDestroyed);
        
        if(sequenceQueue.Count <= 0) return;
        
        sequenceQueue.Dequeue().Invoke();
    }

    private void PlayFirstSequence()
    {
        minimumLinks = 1;
        expectedStartLinkables.Clear();
        expectedEndLinkables.Clear();
        
        expectedStartLinkables.Add(machines[0]);
        expectedEndLinkables.Add(machines[1]);
        
        CreateLine(machines[0], machines[1]);
        
        EventManager.AddListener<MachineEndWorkEvent>(GoToNextStepOnWorkComplete);
        EventManager.AddListener<LinkDestroyedEvent>(NoLineDuringWork);
        
        tutorialCanvas.PlayFirstSequence();
        
        sequenceQueue.Enqueue(PlaySecondSequence);
    }

    private void PlaySecondSequence()
    {
        minimumLinks = 1;
        expectedStartLinkables.Clear();
        expectedEndLinkables.Clear();
        
        expectedStartLinkables.Add(machines[1]);
        expectedEndLinkables.Add(clientSlots[0]);
        
        CreateLine(machines[1], clientSlots[0]);
        EventManager.RemoveListener<LinkDestroyedEvent>(NoLineDuringWork);
        EventManager.AddListener<LinkDestroyedEvent>(GoToNextStepOnCompleteLink);
        
        tutorialCanvas.PlaySecondSequence();
        
        sequenceQueue.Enqueue(PlayThirdSequence);
    }
    
    private void PlayThirdSequence()
    {
        minimumLinks = 2;
        expectedStartLinkables.Clear();
        expectedEndLinkables.Clear();
        
        expectedStartLinkables.Add(machines[0]);
        expectedStartLinkables.Add(machines[2]);
        expectedEndLinkables.Add(machines[2]);
        expectedEndLinkables.Add(clientSlots[0]);
        
        CreateLine(machines[0], machines[2], clientSlots[0]);
        EventManager.AddListener<LinkDestroyedEvent>(GoToNextStepOnCompleteLink);
        
        tutorialCanvas.PlayThirdSequence();
        
        sequenceQueue.Enqueue(PlayFourthSequence);
    }
    
    private void PlayFourthSequence()   
    {
        minimumLinks = 0;
        expectedStartLinkables.Clear();
        expectedEndLinkables.Clear();
        expectedStartLinkables.Add(machines[2]);
        expectedEndLinkables.Add(machines[1]);
        
        magicLineService.CanDestroyLinks(true);
        magicLineService.CreateLink(machines[2],machines[1]);
        expectedStartLinkables.Clear();
        expectedEndLinkables.Clear();
        
        EventManager.AddListener<LinkDestroyedEvent>(GoToNextStepOnLinkDestroyed);

        tutorialLine.enabled = false;
        
        tutorialCanvas.PlayFourthSequence();
        
        sequenceQueue.Enqueue(EndTutorial);
    }

    private void EndTutorial()
    {
        EventManager.Trigger(new EndTutorialEvent());
        
        unlockedItem.ForceUnlock();
        if(ScriptableItemDatabase.CollectionLevel < 1) ScriptableItemDatabase.CollectionLevel = 1;
        
        tutorialCanvas.ShowCursor(false);
        RemoveTutorialModifications(null);
    }

    private void CreateLine(params ILinkable[] linkables)
    {
        tutorialLine.positionCount = linkables.Length;
        for (int i = 0; i < tutorialLine.positionCount; i++)
        {
            var pos = linkables[i].Position;
            pos.y = 1.5f;
            tutorialLine.SetPosition(i,pos);
        }
    }
}

public class LoadTutorialEvent { }
public class EndTutorialEvent { }


