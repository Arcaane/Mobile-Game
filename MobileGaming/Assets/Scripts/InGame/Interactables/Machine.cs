using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public abstract class Machine : MonoBehaviour, ILinkable
{
    [Header("Feedback")]
    [SerializeField] private Image[] feedbackImages;
    [SerializeField] protected TextMeshProUGUI feedbackText;

    [Header("Production Settings")]
    [SerializeField] private float baseTimeToProduce = 5f;
    [SerializeField] private float baseTimeMultiplier = 1f;
    private float timeMultiplier = 1f;
    
    public Vector3 Position => transform.position;
    public virtual bool Inputable => true;
    public virtual bool Outputable => true;
    
    protected double timer { get; private set; }
    protected double waitDuration { get; private set; }
    public Product currentProduct { get; protected set; } = null;
    public bool IsWorking { get; protected set; } = false;

    private void Start()
    {
        ShowProduct(false);
    }

    public void ResetVariables()
    {
        timeMultiplier = 1f;
        
        Setup();
    }

    public void IncreaseTimeMultiplier(float time)
    {
        timeMultiplier = time;
    }

    #region Feedback

    protected void ShowProduct(bool value)
    {
        if (currentProduct == null || !value)
        {
            foreach (var feedbackImage in feedbackImages)
            {
                feedbackImage.color = Color.clear;
            }
            return;
        }
        
        currentProduct.data.ApplySpriteIndexes(feedbackImages[0],feedbackImages[1],feedbackImages[2]);
        
        
    }
    
    protected abstract void Setup();

    #endregion

    #region Work

    protected void StartWork(Product product)
    {
        IsWorking = true;
        currentProduct = product;
        waitDuration = baseTimeToProduce * 1f / (baseTimeMultiplier*timeMultiplier);
        
        StartCoroutine(WorkProduct());
    }

    protected virtual void OnStartWork()
    {
        
    }

    protected virtual void WorkUpdate()
    {
        
    }
    
    private IEnumerator WorkProduct()
    {
        timer = 0;
        
        OnStartWork();
        
        while (timer < waitDuration)
        {
            yield return null;
            timer += Time.deltaTime;
            WorkUpdate();
        }
        
        IsWorking = false;
        
        EndWork();

        TriggerOnAvailable();
    }
    
    protected abstract void EndWork();
    
 
    #endregion

    #region Linkable
    public abstract void SetStartLinkable(Link link);
    public abstract void SetEndLinkable(Link link);
    public abstract bool IsAvailable();
    public event Action OnAvailable;
    protected void TriggerOnAvailable()
    {
        OnAvailable?.Invoke();
    }

    #endregion
}
