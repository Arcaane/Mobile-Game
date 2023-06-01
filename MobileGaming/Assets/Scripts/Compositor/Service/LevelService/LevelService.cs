using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using Service;
using UnityEngine;

public class LevelService : ILevelService
{
    [DependsOnService] private IMagicLineService magicLineService;
    public IMagicLineService MagicLineService => magicLineService;

    public Level CurrentLevel { get; private set; }

    private List<ClientTiming> clientTimings = new();
    private float LevelDuration => extraLevelDuration + baseLevelDuration;
    private float baseLevelDuration;
    private float extraLevelDuration;
    private int scoreToWin;
    private int palier2;
    private int palier3;
    private int clientCount;
    
    //Tout les timings, si le temps correspond au timing, les data vont dans la queue data
    private Queue<ClientTiming> queuedTimings = new(); 

    //Tout les data qui sont dispo, va quand le client dispo de queue client ou bien,quand un client est libre il prend la data dispo
    private Queue<ClientData> queuedData = new();

    //Tout les client qui ont pas de travail et pas de data
    private Queue<ClientSlot> queuedSlots = new(); 
    
    private int currentScore;
    private float currentTime;
    private bool running;

    [ServiceInit]
    private void AddListeners()
    {
        EventManager.AddListener<ClientSlotAvailableEvent>(IncreaseScore);
        EventManager.AddListener<ClientSlotAvailableEvent>(TryDequeueData);
        EventManager.AddListener<ClientSlotAvailableEvent>(TryEndLevel);

        void TryDequeueData(ClientSlotAvailableEvent clientAvailableEvent)
        {
            var slot = clientAvailableEvent.ClientSlot;
            
            if (currentTime < LevelDuration) queuedData.Enqueue(slot.data);
            
            if (queuedData.Count > 0) // data is available
            {
                var data = queuedData.Dequeue();
                slot.SetData(data);
                return;
            }

            // data is not available
            queuedSlots.Enqueue(slot);
        }

        void IncreaseScore(ClientSlotAvailableEvent clientAvailableEvent)
        {
            var data = clientAvailableEvent.Data;
            var scriptable = data.scriptableClient;

            if (clientAvailableEvent.Satisfaction > 0) currentScore += data.Reward;

            UpdateScoreUI();

            foreach (var system in CurrentLevel.FeedbackFx)
            {
                system.gameObject.SetActive(false);
            }

            var fxIndex = 2;
            var percent = (clientAvailableEvent.Satisfaction / data.Satisfaction);
            if (percent <= 0f) fxIndex = 3;
            if (percent >= scriptable.GoodPercent) fxIndex = 1;
            if (percent > scriptable.BrewtifulPercent) fxIndex = 0;

            clientAvailableEvent.ClientSlot.PlayFeedback(fxIndex);
            
            if (fxIndex > 0 && fxIndex < CurrentLevel.FeedbackFx.Length)
            {
                CurrentLevel.FeedbackFx[fxIndex].gameObject.SetActive(true);
                CurrentLevel.FeedbackFx[fxIndex].Play();
            }
        }

        void TryEndLevel(ClientSlotAvailableEvent clientAvailableEvent)
        {
            if (currentTime < LevelDuration) return;

            if(queuedSlots.Count < clientCount) return;
            
            var stars = 0;
            if (currentScore > scoreToWin) stars++;
            if (currentScore > palier2) stars++;
            if (currentScore > palier3) stars++;

            EndLevel(stars);
        }
    }

    private void RemoveListeners()
    {
        EventManager.RemoveListeners<MachineStartWorkEvent>();
        EventManager.RemoveListeners<MachineEndWorkEvent>();
        EventManager.RemoveListeners<ClientSlotAvailableEvent>();
        EventManager.RemoveListeners<ClientDataSetEvent>();
        EventManager.RemoveListeners<LinkCreatedEvent>();
        EventManager.RemoveListeners<LinkDestroyedEvent>();
    }

    public void IncreaseLevelDuration(float amount)
    {
        extraLevelDuration += amount;
    }

    public void InitLevel(Level level)
    {
        CurrentLevel = level;

        CurrentLevel.StartPanel.OnAnimationOver += StartLevel;
        
        ResetVariables();

        LinkStuff();
    }

    private void ResetVariables()
    {
        clientTimings = CurrentLevel.clientTimings.ToList();
        baseLevelDuration = CurrentLevel.levelDuration;
        extraLevelDuration = 0f;
        scoreToWin = CurrentLevel.scoreToWin;
        palier2 = CurrentLevel.palier2;
        palier3 = CurrentLevel.palier3;
        clientCount = CurrentLevel.clientSlots.Count;
        
        RemoveListeners();
        AddListeners();

        foreach (var machine in CurrentLevel.machines)
        {
            machine.ResetVariables();
        }
        
        queuedTimings.Clear();
        queuedData.Clear();
        queuedSlots.Clear();

        currentTime = 0;
        currentScore = 0;
        running = false;

        FillQueuedTimings();
        FillQueuedClients();

        foreach (var fx in CurrentLevel.FeedbackFx)
        {
            fx.gameObject.SetActive(false);
        }

        void FillQueuedTimings()
        {
            clientTimings.Sort();

            foreach (var clientTiming in clientTimings)
            {
                queuedTimings.Enqueue(clientTiming);
            }
        }

        void FillQueuedClients()
        {
            foreach (var client in CurrentLevel.clientSlots)
            {
                queuedSlots.Enqueue(client);
            }
        }
    }

    private void LinkStuff()
    {
        CurrentLevel.StartPanel.UpdateValues(CurrentLevel);
    }

    public void StartLevel()
    {
        SorcererController.Instance.hudCanvasGO.SetActive(true);
        var equippedItems = ScriptableSettings.EquippedItemEffects;
        if (equippedItems.Count > 0)
        {
            foreach (var effect in equippedItems)
            {
                effect.ActivateEffect(this);
            }
        }

        UpdateTimeUI();
        UpdateScoreUI();
        
        magicLineService.Enable();


        EventManager.Trigger(new StartLevelEvent(CurrentLevel));
        
        running = true;
    }

    [OnTick]
    private void Update()
    {
        if (!running) return;

        if (currentTime > LevelDuration)
        {
            return;
        }
        
        IncreaseTime();
    }
    
    private void IncreaseTime()
    {
        currentTime += Time.deltaTime;

        DequeueTimings();

        UpdateTimeUI();

        if (currentTime > LevelDuration)
        {
            queuedTimings.Clear();
            queuedData.Clear();
        }

        void DequeueTimings()
        {
            if(queuedTimings.Count <= 0) return;

            var nextTiming = queuedTimings.Peek().time;
            
            if(currentTime < nextTiming) return;

            var timing = queuedTimings.Dequeue();
            var data = timing.data;
            
            if (queuedSlots.Count > 0)
            {
                queuedSlots.Dequeue().SetData(data);
                return;
            }
            
            queuedData.Enqueue(data);
        }
    }

    private void UpdateTimeUI()
    {
        EventManager.Trigger(new LevelTimeUpdatedEvent(currentTime,LevelDuration,this));
    }
    
    private void UpdateScoreUI()
    {
        EventManager.Trigger(new LevelScoreUpdatedEvent(currentScore,palier3));
    }

    public void EndLevel(int state)
    {
        running = false;
        magicLineService.Disable();

        var equippedItems = ScriptableSettings.EquippedItemEffects;
        if (equippedItems.Count > 0)
        {
            foreach (var effect in equippedItems)
            {
                effect.RemoveEffect(this);
            }
        }
        
        EventManager.Trigger(new EndLevelEvent(CurrentLevel,state));
        CurrentLevel = null;
    }
}

public class LoadLevelEvent
{
    public Level Level { get;}

    public LoadLevelEvent(Level level)
    {
        Level = level;
    }
}

public class StartLevelEvent
{
    public Level Level { get;}

    public StartLevelEvent(Level level)
    {
        Level = level;
    }
}

public class EndLevelEvent
{
    public Level Level { get;}
    public int State { get;}

    public EndLevelEvent(Level level, int state)
    {
        Level = level;
        State = state;
    }
}

public class LevelScoreUpdatedEvent
{
    public int Score { get; }
    public int Palier3 { get; }
    public LevelScoreUpdatedEvent(int score,int palier3)
    {
        Score = score;
        Palier3 = palier3;
    }
}

public class LevelTimeUpdatedEvent
{
    public float CurrentTime { get; }
    public float MaxTime { get; }
    public LevelService Service { get; }
    public LevelTimeUpdatedEvent(float time, float maxTime,LevelService service)
    {
        CurrentTime = time;
        MaxTime = maxTime;
        Service = service;
    }
}