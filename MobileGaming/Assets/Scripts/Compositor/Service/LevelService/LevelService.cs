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
    public float CurrentTime { get; private set; }
    private bool running;

    [ServiceInit]
    private void AddListeners()
    {
        EventManager.AddListener<ClientCompletedEvent>(IncreaseScoreOnClientCompleted);
        EventManager.AddListener<ClientCompletedEvent>(TryDequeueData);
        EventManager.AddListener<ClientCompletedEvent>(TryEndLevelOnClientCompleted);
        EventManager.AddListener<LevelScoreUpdatedEvent>(TryEndLevelOn3Stars);

        void TryDequeueData(ClientCompletedEvent clientAvailableEvent)
        {
            var slot = clientAvailableEvent.ClientSlot;
            
            if (CurrentTime < LevelDuration) queuedData.Enqueue(slot.data);
            
            if (queuedData.Count > 0) // data is available
            {
                var data = queuedData.Dequeue();
                slot.SetData(data);
                return;
            }

            // data is not available
            queuedSlots.Enqueue(slot);
        }

        void IncreaseScoreOnClientCompleted(ClientCompletedEvent clientCompletedEvent)
        {
            var data = clientCompletedEvent.Data;
            var scriptable = data.scriptableClient;

            if (clientCompletedEvent.CurrentSatisfaction > 0) IncreaseScore(data.Reward);
            
            EventManager.Trigger(new LevelScoreUpdatedEvent(currentScore,palier3));

            var fxIndex = 2;
            var percent = (clientCompletedEvent.CurrentSatisfaction / data.Satisfaction);
            if (percent <= 0f) fxIndex = 3;
            if (percent >= scriptable.GoodPercent) fxIndex = 1;
            if (percent > scriptable.BrewtifulPercent) fxIndex = 0;

            clientCompletedEvent.ClientSlot.PlayFeedback(fxIndex);
        }

        void TryEndLevelOnClientCompleted(ClientCompletedEvent clientCompletedEvent)
        {
            if (CurrentTime < LevelDuration) return;

            if(queuedSlots.Count < clientCount) return;
            
            EndLevel();
        }

        void TryEndLevelOn3Stars(LevelScoreUpdatedEvent levelScoreUpdatedEvent)
        {
            if(levelScoreUpdatedEvent.Score > levelScoreUpdatedEvent.Palier3) EndLevel();
        }
    }

    private void RemoveListeners()
    {
        EventManager.RemoveListeners<MachineStartWorkEvent>();
        EventManager.RemoveListeners<MachineEndWorkEvent>();
        EventManager.RemoveListeners<ClientCompletedEvent>();
        EventManager.RemoveListeners<ClientDataSetEvent>();
        EventManager.RemoveListeners<LinkCreatedEvent>();
        EventManager.RemoveListeners<LinkDestroyedEvent>();
    }

    public void IncreaseLevelDuration(float amount)
    {
        extraLevelDuration += amount;
    }

    public void IncreaseScore(int amount)
    {
        currentScore += amount;
        Debug.Log($"Gained {amount} (score is now {currentScore}), need {palier3} to end");
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

        CurrentTime = 0;
        currentScore = 0;
        running = false;

        FillQueuedTimings();
        FillQueuedClients();
        
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

        EventManager.Trigger(new LevelTimeUpdatedEvent(CurrentTime,LevelDuration,this));
        EventManager.Trigger(new LevelScoreUpdatedEvent(currentScore,palier3));
        
        magicLineService.Enable();


        EventManager.Trigger(new StartLevelEvent(CurrentLevel));
        
        running = true;
    }
    
    [OnTick]
    private void Update()
    {
        if (!running) return;

        if (CurrentTime > LevelDuration)
        {
            return;
        }
        
        IncreaseTime();
    }
    
    private void IncreaseTime()
    {
        CurrentTime += Time.deltaTime;

        DequeueTimings();

        EventManager.Trigger(new LevelTimeUpdatedEvent(CurrentTime,LevelDuration,this));

        if (CurrentTime > LevelDuration)
        {
            queuedTimings.Clear();
            queuedData.Clear();
        }

        void DequeueTimings()
        {
            if(queuedTimings.Count <= 0) return;

            var nextTiming = queuedTimings.Peek().time;
            
            if(CurrentTime < nextTiming) return;

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
    
    public void EndLevel()
    {
        if(!running) return;
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
        
        var stars = CalculateScore();
        EventManager.Trigger(new EndLevelEvent(CurrentLevel,stars,currentScore));
        CurrentLevel = null;
    }

    private int CalculateScore()
    {
        var stars = 0;
        if (currentScore > scoreToWin) stars++;
        if (currentScore > palier2) stars++;
        if (currentScore < palier3) return stars;

        var extraTime = CurrentLevel.levelDuration - CurrentTime;
        var extraTimeMilliseconds = (int)Mathf.Ceil(extraTime * 1000);

        if(extraTime > 0) currentScore += extraTimeMilliseconds * CurrentLevel.ScorePerExtraMillisecond;
        
        return 3;
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
    public int Score { get;}

    public EndLevelEvent(Level level, int state,int score)
    {
        Level = level;
        State = state;
        Score = score;
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