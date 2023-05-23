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
    private Slider score;
    private TextMeshProUGUI timeText;

    private List<ClientTiming> clientTimings = new ();
    private List<Client> clients = new ();
    private float levelDuration;
    private int scoreToWin;
    private int palier2;
    private int palier3;
    
    private Queue<ClientTiming> queuedTimings = new(); //Tout les timings, si le temps correspond au timing, les data vont dans la queue data
    private Queue<ClientData> queuedData = new(); //Tout les data qui sont dispo, va quand le client dispo de queue client ou bien,quand un client est libre il prend la data dispo
    private Queue<Client> queuedClient = new(); //Tout les client qui ont pas de travail et pas de data

    private ClientTiming nextTiming;
    private Client availableClient;
    private double startTime;
    private double maxTime = 0;

    private bool stopRefill;

    private int currentScore;
    private float currentTime;
    private bool running;

    public event Action<int> OnEndLevel;

    public void InitLevel(Level level,Slider newScore,TextMeshProUGUI newTimeText)
    {
        currentLevel = level;

        currentLevel.StartPanel.OnAnimationOver += StartLevel;

        score = newScore;
        timeText = newTimeText;
        ResetVariables();

        LinkStuff();
    }
    
    private void ResetVariables()
    {
        OnEndLevel = null;

        clientTimings = currentLevel.clientTimings.ToList();
        clients = currentLevel.clients.ToList();
        levelDuration = currentLevel.levelDuration;
        scoreToWin = currentLevel.scoreToWin;
        palier2 = currentLevel.palier2;
        palier3 = currentLevel.palier3;
        
        queuedTimings.Clear();
        queuedData.Clear();
        queuedClient.Clear();

        currentTime = 0;
        currentScore = 0;
        stopRefill = false;
        running = false;

        FillQueuedTimings();
        
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
    }

    private void LinkStuff()
    {
        currentLevel.StartPanel.UpdateValues(currentLevel);

        magicLineService.SetCamera(currentLevel.Camera);
        
        SubscribeClients();
        
        void SubscribeClients()
        {
            EventManager.AddListener<ClientAvailableEvent>(TryDequeueClient);

            void TryDequeueClient(ClientAvailableEvent clientAvailableEvent)
            {
                if (queuedData.Count > 0) // data is available
                {
                    var data = queuedData.Dequeue();
                    clientAvailableEvent.Client.SetData(data);  
                }
            }
            
            foreach (var client in clients)
            {
                
                
                client.OnClientAvailable += UpdateAvailableClient;
                client.OnClientAvailable += TryEndLevel;
                client.OnClientAvailable += IncreaseScore;

                UpdateAvailableClient();

                void TryEndLevel()
                {
                    if (queuedTimings.Count <= 0 && queuedData.Count == clients.Count)
                    {
                        Debug.Log("Ending Level");
                        var stars = 0;
                        if (currentScore > scoreToWin) stars++;
                        if (currentScore > palier2) stars++;
                        if (currentScore > palier3) stars++;

                        EndLevel(stars);
                    }
                }

                void UpdateAvailableClient()
                {
                    // queuedData.Enqueue(client);
                }

                void IncreaseScore()
                {
                    var data = client.data;
                    var scriptable = data.scriptableClient;

                    if (client.Satisfaction > 0) currentScore += data.Reward;

                    UpdateScoreUI();

                    foreach (var system in currentLevel.FeedbackFx)
                    {
                        system.gameObject.SetActive(false);
                    }

                    var fxIndex = 2;
                    var percent = (client.Satisfaction / data.Satisfaction);
                    if (percent <= 0f) fxIndex = 3;
                    if (percent >= scriptable.GoodPercent) fxIndex = 1;
                    if (percent > scriptable.BrewtifulPercent) fxIndex = 0;


                    currentLevel.FeedbackFx[fxIndex].gameObject.SetActive(true);
                    currentLevel.FeedbackFx[fxIndex].Play();
                }
            }
        }
    }

    public void StartLevel()
    {
        SorcererController.Instance.hudCanvasGO.SetActive(true);
        
        startTime = Time.time;
        
        UpdateTimeUI();
        UpdateScoreUI();

        magicLineService.Enable();
        
        //running = true;
    }
    
    [OnTick]
    private void Update()
    {
        if(!running) return;
        
        UpdateQueue();
        IncreaseTime();
    }

    public void EndLevel()
    {
        EventManager.Trigger(currentLevel);
        currentLevel = null;
    }
    
    private void UpdateQueue()
    {
        //if (!queuedTimings.TryPeek(out nextTiming) || !queuedData.TryPeek(out availableClient)) return;
        
        if(Time.time - startTime < nextTiming.time) return;
        
        queuedTimings.Dequeue();
        queuedData.Dequeue();
        availableClient.SetData(nextTiming.data);  

        nextTiming.time += (float) maxTime;
        if(!stopRefill) queuedTimings.Enqueue(nextTiming);
    }

    private void IncreaseTime()
    {
        currentTime += Time.deltaTime;

        UpdateTimeUI();
        
        if(!stopRefill) TryStopRefill();
    }

    private void UpdateTimeUI()
    {
        var time = levelDuration - currentTime;
        timeText.text = $"Time Left : {(time >= 0 ? time : "Extra time !"):f0}";
    }
    
    private void TryStopRefill()
    {
        if(currentTime < levelDuration) return;
        
        // Fin du timer
        queuedData.Clear();
        stopRefill = true;
        Debug.Log("Stop Refill");
        UpdateScoreUI();
    }
    
    private void UpdateScoreUI()
    {
        score.value = (float)currentScore / palier3;
    }

    private void EndLevel(int state)
    {
        running = false;
        OnEndLevel?.Invoke(state);
    }
}

public class LoadLevelEvent
{
    public Level Level { get; private set; }

    public LoadLevelEvent(Level level)
    {
        Level = level;
    }
}

public class EndLevelEvent
{
    public Level Level { get; private set; }
    public int State { get; private set; }

    public EndLevelEvent(Level level, int state)
    {
        Level = level;
    }
}