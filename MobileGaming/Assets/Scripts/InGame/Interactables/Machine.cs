using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Machine : MonoBehaviour, ILinkable
{
    [Header("Linkable")]
    [SerializeField] private float width;
    public float Width => width;
    
    [Header("Feedback")]
    [SerializeField] private Image[] feedbackImages;
    [SerializeField] protected TextMeshProUGUI feedbackText;
    [SerializeField] protected GameObject selectedFeedbackGo;

    [field:Header("Production Settings")]
    [field: SerializeField] public float BaseTimeToProduce { get; private set; } = 5f;
    public float TimeToProduce => BaseTimeToProduce * 1/(BaseSpeed+bonusSpeed);
    public abstract ProductShape MachineShape { get; }
    public abstract ProductColor MachineColor { get; }
    public abstract ProductTopping machineTopping { get; }

    [field: SerializeField] public float BaseSpeed { get; private set; } = 1f;
    private float bonusSpeed = 0;
    
    public Vector3 Position => transform.position;
    public virtual bool Inputable => true;
    public virtual bool Outputable => true;
    
    protected double timer { get; private set; }
    public Product currentProduct { get; protected set; } = null;
    public bool IsWorking { get; protected set; } = false;

    private void Start()
    {
        ShowProduct(false);
    }

    public void ResetVariables()
    {
        bonusSpeed = 0f;
        selectedFeedbackGo.SetActive(false);
        Setup();
    }

    public void IncreaseBonusSpeed(float amount)
    {
        var ratio = timer / TimeToProduce;
        bonusSpeed += amount;
        timer = TimeToProduce * ratio;
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
        EventManager.Trigger(new MachineStartWorkEvent(this));

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
        
        while (timer < TimeToProduce)
        {
            yield return null;
            timer += Time.deltaTime;
            WorkUpdate();
        }
        
        IsWorking = false;
        
        EndWork();
        EventManager.Trigger(new MachineEndWorkEvent(this));

        TriggerOnAvailable();
    }
    
    protected abstract void EndWork();
    
 
    #endregion

    #region Linkable

    public void ShowHighlight(bool value)
    {
        selectedFeedbackGo.SetActive(value);
    }

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

public class MachineStartWorkEvent
{
    public Machine Machine { get; }
    
    public MachineStartWorkEvent(Machine machine)
    {
        Machine = machine;
    }
}

public class MachineEndWorkEvent
{
    public Machine Machine { get; }
    
    public MachineEndWorkEvent(Machine machine)
    {
        Machine = machine;
    }
}
