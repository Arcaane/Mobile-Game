using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Level : MonoBehaviour
{
    public int currentChapter;
    public int currentLevel;
    
    
    [field:Header("Components")]
    [field:SerializeField] public LevelStartPannel StartPanel { get; private set; }
    [field:SerializeField] public Camera Camera { get; private set; }
    [field:SerializeField] public ParticleSystem[] FeedbackFx { get; private set; }
    
    [HideInInspector,SerializeField] public float levelDuration;

    [HideInInspector,SerializeField] public int scoreToWin;
    [HideInInspector,SerializeField] public int palier2;
    [HideInInspector,SerializeField] public int palier3;

    public List<ClientTiming> clientTimings = new ();

    [Header("Setup with tool automatically")]
    public List<Client> clients = new ();
    private void Start()
    {
        EventManager.Trigger(new LoadLevelEvent(this));
        EventManager.RemoveListeners<LoadLevelEvent>();
    }
    
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
            
            level.scoreToWin = EditorGUILayout.IntField("Score to Win", level.scoreToWin);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Paliers :");
            level.scoreToWin = EditorGUILayout.IntField(level.scoreToWin);
            level.palier2 = EditorGUILayout.IntField(level.palier2);
            level.palier3 = EditorGUILayout.IntField(level.palier3);
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
        return $"Timing at {time} ({data})";
    }
}

