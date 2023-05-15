using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public partial class Level : MonoBehaviour
{
    public int currentChapter;
    public int currentLevel;
    
    [field:SerializeField] public Camera Camera { get; private set; }
    [field:SerializeField] public ParticleSystem[] FeedbackFx { get; private set; }
    
    [HideInInspector,SerializeField] public float levelDuration;
    [HideInInspector,SerializeField] private float currentTime;

    [HideInInspector,SerializeField] public int scoreToWin;
    [HideInInspector,SerializeField] public int currentScore;
    [HideInInspector,SerializeField] public float palier2;
    [HideInInspector,SerializeField] public float palier3;

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
    private bool Running => canRun && loaded;
    public bool canRun;
    private bool loaded;

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI timeText;

    public static event Action<Level> OnLevelLoad;

    private void Start()
    {
        Debug.Log($"Spawned level {this}");
        
        OnLevelLoad?.Invoke(this);
        OnLevelLoad = null;
    }
    
    public void Run()
    {
        Setup();
        
        UpdateTimeUI();
        UpdateScoreUI();
        
        loaded = true;
    }
    
    
    private void Setup()
    {
        OnEndLevel = null;
        
        queuedTimings.Clear();
        queuedClients.Clear();

        currentTime = 0;
        currentScore = 0;
        stopRefill = false;
        loaded = false;
        
        SetupQueues();
        
        SubscribeClients();
        
        startTime = Time.time;

        foreach (var fx in FeedbackFx)
        {
            fx.gameObject.SetActive(false);
        }
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
                var scriptable = data.scriptableClient;
        
                if(client.Satisfaction > 0) currentScore += data.Reward;
        
                UpdateScoreUI();
                
                foreach (var system in FeedbackFx)
                {
                    system.gameObject.SetActive(false);
                }
                
                var fxIndex = 2;
                var percent = (client.Satisfaction / data.Satisfaction);
                if (percent <= 0f) fxIndex = 3;
                if (percent >= scriptable.GoodPercent) fxIndex = 1;
                if (percent > scriptable.BrewtifulPercent) fxIndex = 0;
                

                FeedbackFx[fxIndex].gameObject.SetActive(true);
                FeedbackFx[fxIndex].Play();
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
        if(!Running) return;
        UpdateQueue();
        IncreaseTime();
    }

    //TODO - Sortir de l'update + clear la queue quand le timer arrive (y'a un truc qui marche mais ca peut etre mieu)
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
        
        queuedClients.Clear();
        
        stopRefill = true;
    }
    
    private void UpdateScoreUI()
    {
        scoreText.text = $"$$ : {currentScore}/{scoreToWin}";
    }

    private void EndLevel(int state)
    {
        loaded = false;
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

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Paliers :");
            level.scoreToWin = EditorGUILayout.IntField(level.scoreToWin);
            level.palier2 = EditorGUILayout.FloatField(level.palier2);
            level.palier3 = EditorGUILayout.FloatField(level.palier3);
            EditorGUILayout.EndHorizontal();

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
                    productData.Topping = (ProductTopping)EditorGUILayout.EnumPopup(productData.Topping);
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

