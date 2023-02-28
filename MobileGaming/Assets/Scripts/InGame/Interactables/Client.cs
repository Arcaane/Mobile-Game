using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    [Header("Feedback")]
    [SerializeField] private Image feedbackImage;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Satisfaction Settings")]
    public float baseSatisfaction = 45f;
    private float currentSatisfaction = 0;
    [SerializeField] private float decayPerSecond = 1f;
    [SerializeField] private float increaseOnProduct;
    
    [Header("Product Settings")]
    public ClientData data;
    private ProductData expectedData => data.productDatas[currentDataIndex];
    
    private int currentDataIndex = 0;
    private Product feedbackProduct;

    private Coroutine satisfactionRoutine;
    private WaitForSeconds satisfactionWait = new WaitForSeconds(0.1f);

    private void Start()
    {
        UpdateFeedbackImage();
        feedbackText.text = string.Empty;
    }

    public Product ReceiveProduct(Product product)
    {
        if (product.data != expectedData) return product;
        
        NextProduct();
        return null;

    }

    private void NextProduct()
    {
        currentDataIndex++;
        currentSatisfaction = baseSatisfaction;
        
        feedbackText.text = "Yay";
        
        StartCoroutine(NewProductDelayRoutine());
        
        IEnumerator NewProductDelayRoutine()
        {
            yield return new WaitForSeconds(0.5f); // TODO - prob mettre l'expected data a null pendant cette periode

            satisfactionRoutine = StartCoroutine(SatisfactionRoutine());
            
            if (currentDataIndex >= data.productDatas.Count)
            {
                IncreasePoints();
                StopClient();
                yield break;
            }
            feedbackText.text = $"{data.name} : \n{expectedData.Color} and {expectedData.Shape}";  
        }

        IEnumerator SatisfactionRoutine()
        {
            while (currentSatisfaction > 0)
            {
                yield return satisfactionWait;
                currentSatisfaction -= 0.1f * decayPerSecond;
                UpdateFeedbackImage();
            }
            
            StopClient();
        }
    }

    public void StopClient()
    {
        OnClientAvailable?.Invoke();
        
        StopCoroutine(satisfactionRoutine);
        satisfactionRoutine = null;
        currentSatisfaction = 0;
        UpdateFeedbackImage();
    }

    private void IncreasePoints()
    {
        //TODO - Increase Points
    }

    public void SetData(ClientData newData)
    {
        data = newData;
        currentDataIndex = -1;
        
        NextProduct();
    }

    public event Action OnClientAvailable;

    public void UpdateFeedbackImage()
    {
        feedbackImage.fillAmount = currentSatisfaction / baseSatisfaction;
    }
}

[Serializable]
public struct ClientData
{
    public string name;
    public List<ProductData> productDatas;
}
