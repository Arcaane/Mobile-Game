using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using Service;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelService : ILevelService
{
    [DependsOnService] private IMagicLineService magicLineService;

    private Level currentLevel;

    private List<ClientTiming> clientTimings = new();
    private float levelDuration;
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
            
            if (queuedData.Count > 0) // data is available
            {
                var data = queuedData.Dequeue();
                queuedData.Enqueue(data);
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

            foreach (var system in currentLevel.FeedbackFx)
            {
                system.gameObject.SetActive(false);
            }

            var fxIndex = 2;
            var percent = (clientAvailableEvent.Satisfaction / data.Satisfaction);
            if (percent <= 0f) fxIndex = 3;
            if (percent >= scriptable.GoodPercent) fxIndex = 1;
            if (percent > scriptable.BrewtifulPercent) fxIndex = 0;


            if (fxIndex > 0 && fxIndex < currentLevel.FeedbackFx.Length)
            {
                currentLevel.FeedbackFx[fxIndex].gameObject.SetActive(true);
                currentLevel.FeedbackFx[fxIndex].Play();
            }
        }

        void TryEndLevel(ClientSlotAvailableEvent clientAvailableEvent)
        {
            if (currentTime < levelDuration) return;

            if(queuedSlots.Count < clientCount) return;

            Debug.Log("Ending Level");
            var stars = 0;
            if (currentScore > scoreToWin) stars++;
            if (currentScore > palier2) stars++;
            if (currentScore > palier3) stars++;

            EndLevel(stars);
        }
    }

    public void InitLevel(Level level)
    {
        currentLevel = level;

        currentLevel.StartPanel.OnAnimationOver += StartLevel;
        
        ResetVariables();

        LinkStuff();
    }

    private void ResetVariables()
    {
        clientTimings = currentLevel.clientTimings.ToList();
        levelDuration = currentLevel.levelDuration;
        scoreToWin = currentLevel.scoreToWin;
        palier2 = currentLevel.palier2;
        palier3 = currentLevel.palier3;
        clientCount = currentLevel.clients.Count;
        
        queuedTimings.Clear();
        queuedData.Clear();
        queuedSlots.Clear();

        currentTime = 0;
        currentScore = 0;
        running = false;

        FillQueuedTimings();
        FillQueuedClients();

        foreach (var fx in currentLevel.FeedbackFx)
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
            foreach (var client in currentLevel.clients)
            {
                queuedSlots.Enqueue(client);
            }
        }
    }

    private void LinkStuff()
    {
        currentLevel.StartPanel.UpdateValues(currentLevel);
    }

    public void StartLevel()
    {
        SorcererController.Instance.hudCanvasGO.SetActive(true);

        UpdateTimeUI();
        UpdateScoreUI();
        
        magicLineService.Enable();
        
        running = true;
    }

    [OnTick]
    private void Update()
    {
        if (!running) return;
        
        if(currentTime > levelDuration) return;
        
        IncreaseTime();
    }
    
    private void IncreaseTime()
    {
        currentTime += Compositor.DeltaTick;

        DequeueTimings();

        UpdateTimeUI();

        if (currentTime > levelDuration)
        {
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
        EventManager.Trigger(new LevelTimeUpdatedEvent(currentTime,levelDuration));
    }
    
    private void UpdateScoreUI()
    {
        EventManager.Trigger(new LevelScoreUpdatedEvent(currentScore,palier3));
    }

    public void EndLevel(int state)
    {
        running = false;
        magicLineService.Disable();
        EventManager.Trigger(new EndLevelEvent(currentLevel,state));
        currentLevel = null;
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
    public LevelTimeUpdatedEvent(float time, float maxTime)
    {
        CurrentTime = time;
        MaxTime = maxTime;
    }
}