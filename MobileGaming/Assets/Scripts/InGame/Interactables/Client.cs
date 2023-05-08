using System;
using System.Collections;
using Unity.Mathematics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Client : MonoBehaviour, ILinkable
{
    [Header("Feedback")]
    [SerializeField] private GameObject feedbackGo;
    [SerializeField] private Image feedbackImage;
    [SerializeField] private Image contentImage;
    [SerializeField] private Image shapeImage;
    
    [SerializeField] private GameObject[] clientGraphsHandler;
    [SerializeField] private ParticleSystem[] emotesFeedback; 


    [HideInInspector] public ClientData data;
    private ProductData expectedData => data.productDatas[currentDataIndex];
    public Vector3 Position => transform.position;
    public bool Inputable => true;
    public bool Outputable => false;

    private float currentSatisfaction = 0;
    public float Satisfaction => currentSatisfaction;
    private bool canReceiveProduct = false;
    
    private int currentDataIndex = 0;
    private Product feedbackProduct;

    private Coroutine satisfactionRoutine;
    private WaitForSeconds satisfactionWait = new (0.1f);
    

    private enum ClientSatisfaction { NewClient, Interrogate, Sleepy, Anger}
    private ClientSatisfaction clientSatisfactionEnum = ClientSatisfaction.NewClient;
    
    private void Start()
    {
        foreach (var t in clientGraphsHandler) t.SetActive(false);
        
        UpdateFeedbackImage();
        feedbackGo.SetActive(false);
        feedbackImage.transform.rotation = Quaternion.Euler(0,0,90f);
    }

    #region Feedback

     private void UpdateFeedbackImage()
    {
        if (data.scriptableClient is null)
        {
            return;
        }
        
        feedbackImage.transform.rotation = Quaternion.Lerp(Quaternion.Euler(80,0,-90f), Quaternion.Euler(80,0,90f), (currentSatisfaction / data.Satisfaction));
    }
    
    private void UpdateUIProductImage()
    {
        feedbackGo.SetActive(true);
        
        var shape = expectedData.Shape;
        var color = expectedData.Color;
        var imageComponent = contentImage;
        var imageComponentShape = shapeImage;
        var settings = ScriptableSettings.GlobalSettings;
        
        switch (shape)
        {
            case ProductShape.Hearth: 
                imageComponentShape.sprite = settings.bottleShapesSprites[0];
                switch (color)
                {
                    case ProductColor.Transparent: Debug.Log("Heart Shape without content"); break;
                    case ProductColor.Blue: imageComponent.sprite = settings.bottleContentSprites[0]; break;
                    case ProductColor.Green: imageComponent.sprite = settings.bottleContentSprites[1]; break;
                    case ProductColor.Red: imageComponent.sprite = settings.bottleContentSprites[2]; break;
                }
                break;
            case ProductShape.Cross: 
                imageComponentShape.sprite = settings.bottleShapesSprites[1];
                switch (color)
                {
                    case ProductColor.Transparent: break;
                    case ProductColor.Blue: imageComponent.sprite = settings.bottleContentSprites[3]; break;
                    case ProductColor.Green: imageComponent.sprite = settings.bottleContentSprites[4]; break;
                    case ProductColor.Red: imageComponent.sprite = settings.bottleContentSprites[5]; break;
                }
                break;
            case ProductShape.Moon:
                imageComponentShape.sprite = settings.bottleShapesSprites[2];
                switch (color)
                {
                    case ProductColor.Transparent: break;
                    case ProductColor.Blue: imageComponent.sprite = settings.bottleContentSprites[6]; break;
                    case ProductColor.Green: imageComponent.sprite = settings.bottleContentSprites[7]; break;
                    case ProductColor.Red: imageComponent.sprite = settings.bottleContentSprites[8]; break;
                }
                break;
        }
    }

    #endregion

    #region Product

    public void SetData(ClientData newData)
    {
        data = newData;
        currentDataIndex = -1;
        
        NextProduct();
    }
    
    public void ReceiveProduct(Product product)
    {
        if (product.data == expectedData)
        {
            emotesFeedback[3].Play();
            
            NextProduct();
            return;
        }
        
        //Todo - Bad Product Feedback
    }
    
    private void NextProduct()
    {
        canReceiveProduct = false;
        
        currentDataIndex++;
        
        if (currentDataIndex >= data.productDatas.Length)
        {
            StopClient();
            return;
        }
        
        currentSatisfaction = data.Satisfaction;
        clientSatisfactionEnum = ClientSatisfaction.NewClient;
        
        foreach (var t in emotesFeedback)
        {
            t.Stop();
        }
        
        feedbackGo.SetActive(false);
        
        StartCoroutine(NewProductDelayRoutine());
        
        IEnumerator NewProductDelayRoutine()
        {
            yield return new WaitForSeconds(0.5f); // TODO - prob mettre l'expected data a null pendant cette periode
            canReceiveProduct = true;
            
            foreach (var t in clientGraphsHandler) { t.SetActive(false); }
            clientGraphsHandler[(int)ClientLook.Parse(data.scriptableClient.clientType.GetType(), data.scriptableClient.clientType.ToString())].SetActive(true);
            
            satisfactionRoutine = StartCoroutine(SatisfactionRoutine());
            
            UpdateUIProductImage();
        }

        IEnumerator SatisfactionRoutine()
        {
            while (currentSatisfaction > 0)
            {
                yield return satisfactionWait;
                currentSatisfaction -= 0.1f * data.SatisfactionDecayPerSecond;
                UpdateFeedbackImage();
            }
            
            StopClient();
        }
    }
    private void StopClient()
    {
        OnClientAvailable?.Invoke();
        
        if(satisfactionRoutine != null) StopCoroutine(satisfactionRoutine);
        satisfactionRoutine = null;
        currentSatisfaction = 0;
        UpdateFeedbackImage();
    }
    
    public event Action OnClientAvailable;
    #endregion

    public float currentSDebug;
    private void UpdateFeedbackImage()
    {
        
        currentSDebug = (currentSatisfaction / data.Satisfaction);
        
        
        if (clientSatisfactionEnum == ClientSatisfaction.NewClient && currentSatisfaction / data.Satisfaction < 0.9f)
        {
            emotesFeedback[0].Play();
            clientSatisfactionEnum = ClientSatisfaction.Interrogate;
        }
        
        if (clientSatisfactionEnum == ClientSatisfaction.Interrogate && currentSatisfaction / data.Satisfaction < 0.6f)
        {
            emotesFeedback[1].Play();
            clientSatisfactionEnum = ClientSatisfaction.Sleepy;
        }
        
        if (clientSatisfactionEnum == ClientSatisfaction.Sleepy && currentSatisfaction / data.Satisfaction < 0.3f)
        {
            emotesFeedback[2].Play();
            clientSatisfactionEnum = ClientSatisfaction.Anger;
        }
    }
    
    public void SetEndLinkable(MachineLink link)
    {
        link.OnComplete += ReceiveProduct;
    }

    public bool IsAvailable(MachineLink link) => true;

    public event Action OnAvailable;

    #endregion
    
    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(Client)),CanEditMultipleObjects]
    public class ClientEditor : Editor
    {
        private int productDataCount = 0;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var client = (Client)target;
            
            EditorGUILayout.LabelField("Product Settings",EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            client.data.scriptableClient = EditorGUILayout.ObjectField("Client", client.data.scriptableClient,typeof(ScriptableClient),true) as ScriptableClient;
            EditorGUILayout.EndHorizontal();
            
            productDataCount = EditorGUILayout.IntField("Product Count", productDataCount);

            if (client.data.scriptableClient is null) return;
            
            var currentLenght = client.data.productDatas.Length;
            
            if (currentLenght != productDataCount)
            {
                var data = new ProductData[productDataCount];
                
                for (int i = 0; i < (currentLenght < productDataCount ? currentLenght : productDataCount); i++)
                {
                    data[i] = client.data.productDatas[i];
                }

                client.data.productDatas = data;
            }

            for (var index = 0; index < productDataCount; index++)
            {
                var productData = client.data.productDatas[index];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel($"Product {index}");
                productData.Color = (ProductColor)EditorGUILayout.EnumPopup(productData.Color);
                productData.Shape = (ProductShape)EditorGUILayout.EnumPopup(productData.Shape);
                EditorGUILayout.EndHorizontal();
                client.data.productDatas[index] = productData;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(15);
            if (GUILayout.Button("+",GUILayout.Width(25)))
            {
                productDataCount++;
            }
            if (GUILayout.Button("-",GUILayout.Width(25)))
            {
                if(productDataCount>= 1) productDataCount--;
            }
            EditorGUILayout.EndHorizontal();
            
        }
    }
#endif
    #endregion
}

[Serializable]
public struct ClientData
{
    public string name => scriptableClient.DisplayName;
    public ScriptableClient scriptableClient;
    public ProductData[] productDatas;

    public float Satisfaction => scriptableClient.BaseTimer + (productDatas.Length > 1 ? (productDatas.Length - 1) * scriptableClient.IncrementalTimer : 0);
    public float SatisfactionDecayPerSecond => scriptableClient.TimerDecayPerSecond;
    public int Reward => scriptableClient.BaseReward + (productDatas.Length > 1 ? (productDatas.Length - 1) * scriptableClient.IncrementalReward : 0);
}

public enum ClientLook
{
    Frog,
    Peasant
}
