using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Machine : MonoBehaviour, ILinkable
{
    [Header("Feedback")]
    [SerializeField] private Image feedbackImage;
    [SerializeField] private GameObject feedbackObject; //TODO Make only one object (one per product), not one per machine and teleport it
    [SerializeField] protected TextMeshProUGUI feedbackText;
    
    [Header("Production Settings")]
    [SerializeField] private float baseTimeToProduce = 5f;
    [SerializeField] private float timeMultiplier = 1f;
    
    public Transform tr => transform;
    public virtual bool Inputable => true;
    public virtual bool Outputable => true;
    private Coroutine workRoutine;
    
    protected double timer { get; private set; }
    protected double waitDuration { get; private set; }
    public Product currentProduct { get; protected set; } = null;
    
    private void Start()
    {
        UpdateFeedbackObject();
        UpdateFeedbackText(0);
        StartFeedback();
    }

    public abstract void StartFeedback();

    public virtual void LoadProduct(Product inProduct, out Product outProduct)
    {
        outProduct = inProduct;
        if (workRoutine is not null) return;
        
        if (inProduct is not null) if(!IsValidInputProduct(inProduct)) return;
        
        UnloadProduct(out outProduct);
        currentProduct = null;
        
        if (inProduct is not null)
        {
            LoadProduct(inProduct);
        }
    }

    public abstract bool IsValidInputProduct(Product product);


    public virtual void LoadProduct(Product product)
    {
        currentProduct = product;
        waitDuration = baseTimeToProduce * 1f / timeMultiplier;

        workRoutine = StartCoroutine(WorkProduct());
    }

    private IEnumerator WorkProduct()
    {
        timer = 0;
        while (timer < waitDuration)
        {
            yield return null;
            timer += Time.deltaTime;
            
            UpdateFeedbackText(1 - timer/waitDuration);
            
            UpdateFeedbackObject();
        }
        
        Work();
        
        EndWork();
    }

    protected abstract void Work();

    private void EndWork()
    {
        UpdateFeedbackText(0);
        
        UpdateFeedbackObject();

        InvokeEndWork();

        workRoutine = null;
    }
    
    public event Action OnEndWork;

    protected void InvokeEndWork()
    {
        OnEndWork?.Invoke();
        
        OnOutput?.Invoke(currentProduct);
    }

    public virtual void UnloadProduct(out Product outProduct)
    {
        outProduct = currentProduct;
        
        UpdateFeedbackText(0);
        
        UpdateFeedbackObject();
    }

    private void UpdateFeedbackText(double amount)
    {
        if(feedbackImage == null) return;
        feedbackImage.fillAmount = (float)amount;
    }

    private void UpdateFeedbackObject()
    {
        if(feedbackObject == null) return;
        feedbackObject.SetActive(currentProduct != null);
    }
    
    public void Ping()
    {
        PrePing();
        if(currentProduct != null && workRoutine == null) EndWork();
    }

    protected virtual void PrePing()
    {
        
    }

    public void Output(out Product product)
    {
        UnloadProduct(out product);
        currentProduct = null;
    }

    public event Action<Product> OnOutput;
    public void Input(Product product)
    {
        Debug.Log($"Input Received ({product})");
        
        LoadProduct(product);
    }

    public event Action<Product> OnInput;
}
