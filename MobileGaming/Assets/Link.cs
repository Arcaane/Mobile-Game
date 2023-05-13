using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Link : MonoBehaviour
{
    #region Variables
    
    [field:SerializeField] public LineRenderer LineRenderer { get; private set; }
    [field:SerializeField] public MeshCollider Collider { get; private set; }
    [FormerlySerializedAs("debugImage")] public Transform bottleImage;
    private DrawMagicLine lineInCollision;
    public Material myMaterial;

    public ILinkable StartLinkable { get; private set; }
    public ILinkable EndLinkable { get; private set; }
    public Vector2[] Points { get; private set; }
    
    // Magic Transportation
    [Range(0,100)] [SerializeField] private int itemProgression = 0;
    public float timeToCompleteTransportation = 5f;
    public float currentTimer = 0f;
    public TextMeshProUGUI lineGroupNumberText;
    public int lineGroupNumber;
    public Product productInTreatment { get; private set; }
    private List<Link> dependentLinks = new List<Link>();
    
    private static readonly int FilingValue = Shader.PropertyToID("_FilingValue");
    #endregion

    private void Start()
    {
        var myMaterial = GetComponent<LineRenderer>();
        
        dependentLinks.Clear();
        lineGroupNumberText.text = lineGroupNumber.ToString();
        lineGroupNumberText.gameObject.transform.position = (StartLinkable.Position + EndLinkable.Position) / 2 + Vector3.up * 2;
    }
    
    #region Feedback

    private void Feedback()
    {
        myMaterial.SetFloat(FilingValue, 1 - currentTimer / timeToCompleteTransportation);
        
        bottleImage.position = Vector3.Lerp(StartLinkable.Position + Vector3.up, 
            EndLinkable.Position + Vector3.up, currentTimer / timeToCompleteTransportation);
        
    }
    
    private void SetUIProduct()
    {
        // Forme de la bouteille
        if(productInTreatment == null) return;
        
        var shape = productInTreatment.data.Shape;
        var color = productInTreatment.data.Color;
        var imageComponentShape = bottleImage.transform.GetChild(0).GetComponent<Image>();
        var imageComponent = bottleImage.transform.GetChild(1).GetComponent<Image>();
        var settings = ScriptableSettings.GlobalSettings;

        
        switch (shape)
        {
            case ProductShape.Hearth: 
                imageComponentShape.sprite = settings.bottleShapesSprites[0];
                switch (color)
                {
                    case ProductColor.Transparent: break;
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
    
    #region ProductTransfer

    public void LoadProduct(Product product)
    {
        productInTreatment = product;
        
        if (productInTreatment == null) return;
        
        StartCoroutine(MoveProductRoutine());
    }
    
    private IEnumerator MoveProductRoutine()
    {
        currentTimer = 0f;
        
        bottleImage.position = Vector3.Lerp(StartLinkable.Position + Vector3.up, 
            EndLinkable.Position + Vector3.up, currentTimer / timeToCompleteTransportation);
        SetUIProduct();
        
        currentTimer += Time.deltaTime;
        while (currentTimer < timeToCompleteTransportation)
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
        
        OnComplete?.Invoke(productInTreatment);
        
        productInTreatment = null;
        
        Destroy();
        
        Destroy(gameObject);
    }

    #endregion
    
    public void SetLinks(ILinkable startLink,ILinkable endLink)
    {
        if(!startLink.Outputable || !endLink.Inputable) return;
        
        StartLinkable = startLink;
        EndLinkable = endLink;
        
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

    public void Destroy()
    {
        OnDestroyed?.Invoke();
        
        Destroy(gameObject);
    }

    public bool CompareLinks(ILinkable start,ILinkable end)
    {
        return (StartLinkable == start && EndLinkable == end);
    }

    public void SetPoints(Camera cam)
    {
        var mesh = new Mesh();
        
        LineRenderer.BakeMesh(mesh,cam);

        Collider.sharedMesh = mesh;
        
        Points = new Vector2[LineRenderer.positionCount];

        var vec3 = new Vector3[Points.Length];
        
        LineRenderer.GetPositions(vec3);

        for (var index = 0; index < Points.Length; index++)
        {
            var point = Points[index];
            point.y = vec3[index].z;
            Points[index] = point;
        }
    }
}
