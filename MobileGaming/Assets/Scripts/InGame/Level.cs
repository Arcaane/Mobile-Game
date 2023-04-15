using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SceneManagement;
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
    private Queue<Client> queuedClients = new();

    private ClientTiming nextTiming;
    private Client availableClient;
    private double startTime;
    private double maxTime = 0;
    
    private bool stopRefill;
    private bool running;

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI timeText;
    
    private void Setup()
    {
        OnEndLevel = null;
        
        queuedTimings.Clear();
        queuedClients.Clear();

        currentTime = 0;
        currentScore = 0;
        stopRefill = false;
        running = false;
        
        SetupQueues();
        
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

    private void SetupQueues()
    {
        clientTimings.Sort();
        maxTime = 0;
        foreach (var clientData in clientTimings)
        {
            queuedTimings.Enqueue(clientData);
            if (maxTime < clientData.time) maxTime = clientData.time;
        }
        Debug.Log($"Enqueued {queuedTimings.Count} timings");
    }

    private void SubscribeClients()
    {
        foreach (var client in clients)
        {
            client.OnClientAvailable += UpdateAvailableClient;
            client.OnClientAvailable += TryEndLevel;
            client.OnClientAvailable += IncreaseScore;
            
            UpdateAvailableClient();
            
            void TryEndLevel()
            {
                if (queuedTimings.Count <= 0 && queuedClients.Count == clients.Count)
                {
                    Debug.Log("Ending Level");
                    EndLevel((currentScore < scoreToWin) ? 0 : 1);
                }
            }
            
            void UpdateAvailableClient()
            {
                queuedClients.Enqueue(client);
            }
            
            void IncreaseScore()
            {
                var data = client.data;
        
                currentScore += data.Reward;
        
                UpdateScoreUI();
            }
        }
        
        Debug.Log($"Enqueued {queuedClients.Count} clients");
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
        if (!queuedTimings.TryPeek(out nextTiming) || !queuedClients.TryPeek(out availableClient)) return;
        
        if(Time.time - startTime < nextTiming.time) return;
        
        queuedTimings.Dequeue();
        queuedClients.Dequeue();
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
        timeText.text = $"Time Left : {(time >= 0 ? time : 0):f0}";
    }
    
    private void TryStopRefill()
    {
        if(currentTime < levelDuration) return;

        stopRefill = true;
    }
    
    private void UpdateScoreUI()
    {
        scoreText.text = $"$$ : {currentScore}/{scoreToWin}";
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
        private Level level;

        private void OnEnable()
        {
            level = (Level)target;
            PrefabUtility.RecordPrefabInstancePropertyModifications(level);
        }
        public override void OnInspectorGUI()
        {
            if (EditorApplication.isPlaying) Undo.RecordObject(level, "descriptive name of this operation");
            
            var script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
            
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
                
                timing.data.scriptableClient = EditorGUILayout.ObjectField("Client",timing.data.scriptableClient,typeof(ScriptableClient),true) as ScriptableClient;

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

            if (!EditorApplication.isPlaying)
            {
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                {
                    EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                }
                else
                {
                    EditorUtility.SetDirty(level);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                    EditorSceneManager.MarkSceneDirty(level.gameObject.scene);
                }
            }
            
            
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
