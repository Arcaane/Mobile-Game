using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Machine : MonoBehaviour, ILinkable
{
    [Header("Feedback")]
    [SerializeField] private Image[] feedbackImages;
    [SerializeField] private GameObject feedbackObject; //TODO Make only one object (one per product), not one per machine and teleport it
    [SerializeField] protected TextMeshProUGUI feedbackText;
    
    [Header("Production Settings")]
    [SerializeField] private float baseTimeToProduce = 5f;
    [SerializeField] private float timeMultiplier = 1f;
    
    public Vector3 Position => transform.position;
    public virtual bool Inputable => true;
    public virtual bool Outputable => true;
    
    protected double timer { get; private set; }
    protected double waitDuration { get; private set; }
    public Product currentProduct { get; protected set; } = null;
    public bool IsWorking { get; protected set; } = false;

    private void Start()
    {
        UpdateFeedbackObject();
        UpdateFeedbackImage(0);
        
        Setup();
    }

    #region Feedback

    private void UpdateFeedbackImage(double amount)
    {
        foreach (var feedbackImage in feedbackImages)
        {
            feedbackImage.fillAmount = (float)amount;
        }
    }

    private void UpdateFeedbackObject()
    {
        if(feedbackObject == null) return;
        feedbackObject.SetActive(currentProduct != null);
    }
    protected abstract void Setup();

    #endregion

    #region Work

    protected void StartWork(Product product)
    {
        IsWorking = true;
        currentProduct = product;
        waitDuration = baseTimeToProduce * 1f / timeMultiplier;

        StartCoroutine(WorkProduct());
    }
    
    private IEnumerator WorkProduct()
    {
        timer = 0;
        
        UpdateFeedback();
        
        while (timer < waitDuration)
        {
            yield return null;
            timer += Time.deltaTime;
            
            UpdateFeedbackImage(1 - timer/waitDuration);
            
            UpdateFeedbackObject();
        }
        
        UpdateFeedback();

        IsWorking = false;
        
        EndWork();

        TriggerOnAvailable();
    }
    
    protected abstract void EndWork();
    
    private void UpdateFeedback()
    {
        UpdateFeedbackImage(0);
        
        UpdateFeedbackObject();
    }
 
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
