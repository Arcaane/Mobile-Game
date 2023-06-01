using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Link : MonoBehaviour
{
    #region Variables

    [SerializeField] private float height = 1.5f;
    [SerializeField] private Transform canvasTr;
    [field:SerializeField] public LineRenderer LineRenderer { get; private set; }
    [field:SerializeField] public BoxCollider Collider { get; private set; }
    [FormerlySerializedAs("debugImage")] public Transform bottleImage;
    private DrawMagicLine lineInCollision;
    public Material myMaterial;

    public ILinkable StartLinkable { get; private set; }
    public ILinkable EndLinkable { get; private set; }
    public Vector2[] Points { get; private set; }
    
    // Magic Transportation
    [field: SerializeField] public float BaseTimeToCompleteTransportation { get; private set; } = 1.5f;
    private float extraTimeToComplete = 0f;
    private float TimeToCompleteTransportation => BaseTimeToCompleteTransportation * 1f / (BaseTimeToCompleteTransportation+extraTimeToComplete);
    
    private float currentTimer = 0f;
    public Product ProductInTreatment { get; private set; }
    private List<Link> dependentLinks = new List<Link>();

    private bool flaggedForDestruction = false;
    
    private static readonly int FilingValue = Shader.PropertyToID("_FilingValue");
    #endregion
    
    public void IncreaseExtraTimeToComplete(float amount)
    {
        extraTimeToComplete += amount;
    }
    
    #region Feedback

    private void Feedback()
    {
        myMaterial.SetFloat(FilingValue, 1 - currentTimer / TimeToCompleteTransportation);
        
        bottleImage.position = Vector3.Lerp(StartLinkable.Position + Vector3.up, 
            EndLinkable.Position + Vector3.up, currentTimer / TimeToCompleteTransportation);
    }
    
    private void SetUIProduct()
    {
        if(ProductInTreatment == null) return;
        bottleImage.gameObject.SetActive(true);
        
        var shapeImage = bottleImage.transform.GetChild(0).GetComponent<Image>();
        var contentImage = bottleImage.transform.GetChild(1).GetComponent<Image>();
        var topingImage = bottleImage.transform.GetChild(2).GetComponent<Image>();

        ProductInTreatment.data.ApplySpriteIndexes(shapeImage,contentImage,topingImage);
    }

    #endregion
    
    #region ProductTransfer

    public void LoadProduct(Product product)
    {
        ProductInTreatment = product;
        
        if (ProductInTreatment == null) return;
        
        StartCoroutine(MoveProductRoutine());
    }
    
    private IEnumerator MoveProductRoutine()
    {
        currentTimer = 0f;
        
        bottleImage.position = Vector3.Lerp(StartLinkable.Position + Vector3.up, 
            EndLinkable.Position + Vector3.up, currentTimer / TimeToCompleteTransportation);
        SetUIProduct();
        
        currentTimer += Time.deltaTime;
        while (currentTimer < TimeToCompleteTransportation)
        {
            currentTimer += Time.deltaTime;
            
            Feedback();
            
            yield return null;
        }
        
        CompleteTransfer();
        
        Feedback();
    }

    private void CompleteTransfer()
    {
        currentTimer = 0;
        
        OnComplete?.Invoke(ProductInTreatment);
        
        ProductInTreatment = null;
        
        DestroyLink(true);
    }

    #endregion
    
    public void SetLinks(ILinkable startLink,ILinkable endLink)
    {
        if(!startLink.Outputable || !endLink.Inputable) return;
        
        StartLinkable = startLink;
        EndLinkable = endLink;
        
        EventManager.Trigger(new LinkCreatedEvent(this));
        
        if(flaggedForDestruction) return;
        
        extraTimeToComplete = 0f;

        canvasTr.SetParent(null);
        EndLinkable.SetEndLinkable(this);
        StartLinkable.SetStartLinkable(this);
    }
    
    public void AddDependency(Link link)
    {
        if(dependentLinks.Contains(link)) return;
        
        dependentLinks.Add(link);
        
        link.OnDestroyed += RemoveDependency;
        
        void RemoveDependency()
        {
            if(!dependentLinks.Contains(link)) return;
            dependentLinks.Remove(link);
            if(dependentLinks.Count > 0) return;
            enabled = true;
        }
    }
    
    public event Action<Product> OnComplete;
    public event Action OnDestroyed;

    public void DestroyLink(bool completedTransfer = false)
    {
        if(flaggedForDestruction) return;
        OnDestroyed?.Invoke();
        var startedTransfer = (ProductInTreatment == null) || completedTransfer;
        EventManager.Trigger(new LinkDestroyedEvent(this,startedTransfer,completedTransfer));
        flaggedForDestruction = true;
        
        Destroy(canvasTr.gameObject);
        Destroy(gameObject);
    }

    public bool CompareLinks(ILinkable start,ILinkable end)
    {
        return (StartLinkable == start && EndLinkable == end);
    }

    public void CreateMesh()
    {
        var tr = transform;
        tr.position = (StartLinkable.Position + EndLinkable.Position) / 2f;
        tr.forward = (tr.position - StartLinkable.Position).normalized;
        
        var distance = (StartLinkable.Position - EndLinkable.Position).magnitude - StartLinkable.Width - EndLinkable.Width;
        var offset = StartLinkable.Width/2f - EndLinkable.Width/2f;
        Collider.center = new Vector3(0, 0, offset);
        Collider.size = new Vector3(0.5f, 0.5f, distance);
    }
}

public class LinkCreatedEvent
{
    public Link Link { get; }

    public LinkCreatedEvent(Link link)
    {
        Link = link;
    }
}

public class LinkDestroyedEvent
{
    public Link Link { get; }
    public bool StartedTransfer { get; }
    public bool CompletedTransfer { get; }

    public LinkDestroyedEvent(Link link,bool startedTransfer,bool completedTransfer)
    {
        Link = link;
        StartedTransfer = startedTransfer;
        CompletedTransfer = startedTransfer && completedTransfer;
    }
}
