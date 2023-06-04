using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Link : MonoBehaviour
{
    #region Variables

    [SerializeField,ColorUsage(true,true)] private List<Color> colors = new List<Color>();
    [SerializeField] private Transform canvasTr;
    [field:SerializeField] public LineRenderer LineRenderer { get; private set; }
    [field:SerializeField] public BoxCollider Collider { get; private set; }
    [FormerlySerializedAs("debugImage")] public Transform bottleImage;
    private DrawMagicLine lineInCollision;

    public ILinkable StartLinkable { get; private set; }
    public ILinkable EndLinkable { get; private set; }

    // Magic Transportation
    [field: SerializeField] public float BaseTimeToCompleteTransportation { get; private set; } = 1.5f;
    private float TimeToCompleteTransportation => BaseTimeToCompleteTransportation * 1/(BaseTransportationSpeed+extraSpeed);
    [field: SerializeField] public float BaseTransportationSpeed { get; private set; } = 1.5f;
    private float extraSpeed;
    [field: SerializeField] public float BaseCollidingLinksSlowAmount { get; private set; } = 0.5f;
    private float extraCollidingLinksSlowAmount = 0f;
    private float CollidingLinksSlowAmount => BaseCollidingLinksSlowAmount + extraCollidingLinksSlowAmount;
    
    private float currentTimer = 0f;
    
    public Product ProductInTreatment { get; private set; }
    public bool FlaggedForDestruction { get; private set; } = false;
    
    private Material material;

    private List<Link> collisionLinks = new List<Link>();

    public static bool disableCollision = false;
    #endregion
    
    public void IncreaseExtraSpeed(float amount)
    {
        var ratio = currentTimer / TimeToCompleteTransportation;
        extraSpeed += amount;
        currentTimer = TimeToCompleteTransportation * ratio;
    }
    
    public void IncreaseExtraTimeInCollision(float amount)
    {
        extraCollidingLinksSlowAmount += amount;
    }
    
    #region Feedback

    private void Feedback()
    {
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
    
    public void SetLinks(ILinkable startLink,ILinkable endLink,int totalLinksCreated)
    {
        collisionLinks.Clear();

        if(!startLink.Outputable || !endLink.Inputable) return;
        
        StartLinkable = startLink;
        EndLinkable = endLink;
        
        extraSpeed = 0f;
        extraCollidingLinksSlowAmount = 0f;

        ChangeColor(colors[0]);

        EventManager.Trigger(new LinkCreatedEvent(this,totalLinksCreated));
        if(FlaggedForDestruction) return;
        
        EventManager.AddListener<LinkCollisionEvent>(SlowIfCollision);
        EventManager.AddListener<ActivateDestroyModeEvent>(IncreaseColliderSizeInDestroyMode);
        

        canvasTr.SetParent(null);
        EndLinkable.SetInLink(this);
        StartLinkable.SetOutLink(this);
    }

    private void SlowIfCollision(LinkCollisionEvent linkCollisionEvent)
    {
        if(disableCollision) return;
        
        if(linkCollisionEvent.Link != this) return;
        
        foreach (var collidingLink in linkCollisionEvent.CollidingLinks)
        {
            if(!CommonLinkables(collidingLink)) collisionLinks.Add(collidingLink);
        }
        
        if (collisionLinks.Count <= 0) return;
        
        ChangeColor(colors[1]);
        
        var slowSpeed = -CollidingLinksSlowAmount;
        
        EventManager.AddListener<LinkDestroyedEvent>(RemoveListenerOnDestroyed);
        EventManager.AddListener<LinkDestroyedEvent>(OnCollisionLinkDestroyed);
        
        IncreaseExtraSpeed(slowSpeed);
        
        TryReturnToNormal();

        void OnCollisionLinkDestroyed(LinkDestroyedEvent linkDestroyedEvent)
        {
            if(!collisionLinks.Contains(linkDestroyedEvent.Link)) return;

            collisionLinks.Remove(linkDestroyedEvent.Link);
            
            TryReturnToNormal();
        }
        
        void TryReturnToNormal()
        {
            if(collisionLinks.Count > 0) return;
            
            ChangeColor(colors[0]);
            IncreaseExtraSpeed(-slowSpeed);
            EventManager.RemoveListener<LinkDestroyedEvent>(OnCollisionLinkDestroyed);
        }

        void RemoveListenerOnDestroyed(LinkDestroyedEvent linkDestroyedEvent)
        {
            if(linkDestroyedEvent.Link != this) return;
            EventManager.RemoveListener<LinkDestroyedEvent>(OnCollisionLinkDestroyed);
        }
    }

    private void ChangeColor(Color color)
    {
        material = LineRenderer.material;
        material.color = color;
        LineRenderer.material = material;
    }
    
    public event Action<Product> OnComplete;
    public event Action OnDestroyed;

    public void DestroyLink(bool completedTransfer = false)
    {
        if(FlaggedForDestruction) return;
        OnDestroyed?.Invoke();
        var startedTransfer = (ProductInTreatment == null) || completedTransfer;
        EventManager.Trigger(new LinkDestroyedEvent(this,startedTransfer,completedTransfer));
        FlaggedForDestruction = true;
        
        EventManager.RemoveListener<ActivateDestroyModeEvent>(IncreaseColliderSizeInDestroyMode);
        Destroy(canvasTr.gameObject);
        Destroy(gameObject);
    }

    public bool CompareLinks(ILinkable start,ILinkable end)
    {
        return (StartLinkable == start && EndLinkable == end);
    }

    public bool CommonLinkables(Link other)
    {
        return (EndLinkable == other.StartLinkable || EndLinkable == other.EndLinkable) || (StartLinkable == other.StartLinkable || StartLinkable == other.EndLinkable);
    }

    public void CreateMesh()
    {
        var tr = transform;
        var startPos = StartLinkable.Position;
        var endPos = EndLinkable.Position;
        startPos.y = 0;
        endPos.y = 0;
        tr.position = (startPos + endPos) / 2f;
        tr.forward = (tr.position - startPos).normalized;
        
        var distance = (startPos - endPos).magnitude - StartLinkable.Width - EndLinkable.Width;
        var offset = StartLinkable.Width/2f - EndLinkable.Width/2f;
        Collider.center = new Vector3(0, 0, offset);
        Collider.size = new Vector3(1f, 0.5f, distance);
    }

    private void IncreaseColliderSizeInDestroyMode(ActivateDestroyModeEvent activateDestroyModeEvent)
    {
        var colliderSize = Collider.size;
        colliderSize.x = activateDestroyModeEvent.Value ? 2f : 1f;
        Collider.size = colliderSize;
    }
}

public class LinkCreatedEvent
{
    public Link Link { get; }
    public int TotalLinksCreated;

    public LinkCreatedEvent(Link link,int totalLinksCreated)
    {
        Link = link;
        TotalLinksCreated = totalLinksCreated;
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
