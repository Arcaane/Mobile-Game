using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Level : MonoBehaviour
{
    [HideInInspector,SerializeField] private float levelDuration;
    private float currentTime;
    
    [HideInInspector,SerializeField] private int scoreToWin;
    private int currentScore;
    
    [SerializeField] private List<ClientTiming> clientTimings = new ();

    [Header("Setup with tool automatically")]
    public List<Client> clients = new ();
    
    private Queue<ClientTiming> queuedTimings = new ();
    private Queue<Client> availableClients = new();

    private ClientTiming nextTiming;
    private Client availableClient;
    private double startTime;
    private double maxTime = 0;

    private bool running;

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI timeText;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        OnEndLevel = null;
        
        queuedTimings.Clear();
        availableClients.Clear();

        currentTime = 0;
        currentScore = 0;
        running = false;
        
        SetupQueue();
        
        SubscribeClients();
        
        startTime = Time.time;
    }

    public void Run()
    {
        Setup();
        
        UpdateTimeUI();
        UpdateScoreUI();
        
        running = true;
    }

    private void SetupQueue()
    {
        clientTimings.Sort();
        maxTime = 0;
        foreach (var clientData in clientTimings)
        {
            queuedTimings.Enqueue(clientData);
            if (maxTime < clientData.time) maxTime = clientData.time;
        }
    }

    private void SubscribeClients()
    {
        foreach (var client in clients)
        {
            client.OnClientAvailable += UpdateAvailableClient;
            client.OnEnd += IncreaseScore;
            
            UpdateAvailableClient();
            
            void UpdateAvailableClient()
            {
                availableClients.Enqueue(client);
            }
        }
    }

    public void SetUIComponents(TextMeshProUGUI newScoreText,TextMeshProUGUI newTimeText)
    {
        scoreText = newScoreText;
        timeText = newTimeText;
    }

    private void Update()
    {
        if(!running) return;
        UpdateQueue();
        IncreaseTime();
    }

    private void UpdateQueue()
    {
        if (!queuedTimings.TryPeek(out nextTiming) || !availableClients.TryPeek(out availableClient)) return;
        
        if(Time.time - startTime < nextTiming.time) return;
        
        queuedTimings.Dequeue();
        availableClients.Dequeue();
        availableClient.SetData(nextTiming.data);
        
        nextTiming.time += (float) maxTime;
        queuedTimings.Enqueue(nextTiming);
    }

    private void IncreaseTime()
    {
        currentTime += Time.deltaTime;

        UpdateTimeUI();
        
        TryDefeat();
    }

    private void UpdateTimeUI()
    {
        timeText.text = $"Time Left : {(levelDuration - currentTime):f0}";
    }

    private void TryDefeat()
    {
        if(currentTime < levelDuration) return;
        EndLevel(0);
    }

    private void IncreaseScore(int score)
    {
        currentScore += score;
        
        UpdateScoreUI();
        
        TryVictory();
    }

    private void UpdateScoreUI()
    {
        scoreText.text = $"$$ : {currentScore}/{scoreToWin}";
    }

    private void TryVictory()
    {
        if(currentScore < scoreToWin) return;
        EndLevel(1);
    }

    private void EndLevel(int state)
    {
        running = false;
        OnEndLevel?.Invoke(state);
    }

    public event Action<int> OnEndLevel;

    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(Level)),CanEditMultipleObjects]
    public class LevelEditor : Editor
    {
        private int clientTimingCount;
        private int[] clientDataCount = Array.Empty<int>();
        
        public override void OnInspectorGUI()
        {
            var script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
            
            var level = (Level)target;

            EditorGUILayout.LabelField("Level Settings",EditorStyles.boldLabel);

            level.levelDuration = EditorGUILayout.FloatField("Level Duration", level.levelDuration);
            GUI.enabled = false;
            EditorGUILayout.FloatField("Current Duration", level.currentTime);
            GUI.enabled = true;
            level.scoreToWin = EditorGUILayout.IntField("Score to Win", level.scoreToWin);
            GUI.enabled = false;
            EditorGUILayout.IntField("Current Score", level.currentScore);
            GUI.enabled = true;
            clientTimingCount = EditorGUILayout.IntField("Client Count", level.clientTimings.Count);

            if (clientTimingCount != level.clientTimings.Count)
            {
                var dif = level.clientTimings.Count - clientTimingCount;

                if (dif > 0) for (int i = 0; i < dif; i++) RemoveClientTiming();
                else for (int i = 0; i < -dif; i++) AddClientTiming();
            }

            if (clientDataCount.Length != level.clientTimings.Count)
            {
                EditorUtility.SetDirty(target);
                clientDataCount = new int[level.clientTimings.Count];
                for (int i = 0; i < level.clientTimings.Count; i++)
                {
                    clientDataCount[i] = level.clientTimings[i].data.productDatas.Length;
                }
            }

            EditorGUI.indentLevel++;
            for (var timingIndex = 0; timingIndex < level.clientTimings.Count; timingIndex++)
            {
                var timing = level.clientTimings[timingIndex];
                
                EditorGUILayout.LabelField("Client Settings", EditorStyles.boldLabel);

                timing.time = EditorGUILayout.FloatField("Client Time", timing.time);
                
                EditorGUILayout.BeginHorizontal();
                timing.data.name = EditorGUILayout.TextField("Client Name",timing.data.name);
                EditorGUILayout.LabelField("Points",GUILayout.Width(51));
                timing.data.points = EditorGUILayout.IntField(timing.data.points,GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                clientDataCount[timingIndex] = EditorGUILayout.IntField("Product Count", clientDataCount[timingIndex]);
                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    clientDataCount[timingIndex]++;
                }

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    if(clientDataCount[timingIndex] > 0) clientDataCount[timingIndex]--;
                }
                EditorGUILayout.EndHorizontal();
                
                var currentLenght = level.clientTimings[timingIndex].data.productDatas.Length;
                if (currentLenght != clientDataCount[timingIndex])
                {
                    EditorUtility.SetDirty(target);
                    var data = new ProductData[clientDataCount[timingIndex]];
                
                    for (int i = 0; i < (currentLenght < clientDataCount[timingIndex] ? currentLenght : clientDataCount[timingIndex]); i++)
                    {
                        data[i] = level.clientTimings[timingIndex].data.productDatas[i];
                    }
                    level.clientTimings[timingIndex].data.productDatas = data;
                }
                
                for (var index = 0; index < clientDataCount[timingIndex]; index++)
                {
                    var productData =  level.clientTimings[timingIndex].data.productDatas[index];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel($"Product {index}");
                    productData.Color = (ProductColor)EditorGUILayout.EnumPopup(productData.Color);
                    productData.Shape = (ProductShape)EditorGUILayout.EnumPopup(productData.Shape);
                    EditorGUILayout.EndHorizontal();
                    level.clientTimings[timingIndex].data.productDatas[index] = productData;
                }
            }
            EditorGUI.indentLevel=0;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(15);
            if (GUILayout.Button("+",GUILayout.Width(25)))
            {
                AddClientTiming();
            }
            if (GUILayout.Button("-",GUILayout.Width(25)))
            {
                RemoveClientTiming();
            }
            EditorGUILayout.EndHorizontal();
            

            base.OnInspectorGUI();
            
            return;

            void AddClientTiming()
            {
                level.clientTimings.Add(new ClientTiming()
                {
                    data = new ClientData()
                    {
                        productDatas = Array.Empty<ProductData>()
                    }
                });
            }

            void RemoveClientTiming()
            {
                if (level.clientTimings.Count > 0)
                {
                    level.clientTimings.RemoveAt(level.clientTimings.Count-1);
                }
            }
        }
    }
#endif
    #endregion
}

[Serializable]
public class ClientTiming : IComparable<ClientTiming>
{
    public float time;
    public ClientData data;

    public int CompareTo(ClientTiming other)
    {
        return time.CompareTo(other.time);
    }

    public override string ToString()
    {
        return $"Timing at {time}";
    }
}
