using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineLink : MonoBehaviour
{
    #region Variables
    
    //public TextMeshProUGUI debugPercentageText;
    public Transform debugImage;
    private DrawMagicLine lineInCollision;
    public Material myMaterial;

    public ILinkable StartLinkable { get; private set; }
    public ILinkable EndLinkable { get; private set; }
    
    // Magic Transportation
    public Product productInTreatment { get; private set; }
    public float timeToCompleteTransportation = 10f;
    private float currentTimer;
    
    private List<MachineLink> dependentLinks = new List<MachineLink>();
    
    private static readonly int FilingValue = Shader.PropertyToID("_FilingValue");

    #endregion

    private void Start()
    {
        myMaterial = GetComponent<LineRenderer>().material;
        
        dependentLinks.Clear();
    }
    
    #region Feedback

    private void Feedback()
    {
        myMaterial.SetFloat(FilingValue, 1 - currentTimer / timeToCompleteTransportation);
        
        debugImage.position = Vector3.Lerp(StartLinkable.Position + Vector3.up, 
            EndLinkable.Position + Vector3.up, currentTimer / timeToCompleteTransportation);
        
    }
    
    private void SetUIProduct()
    {
        // Forme de la bouteille
        if(productInTreatment == null) return;
        
        var shape = productInTreatment.data.Shape;
        var color = productInTreatment.data.Color;
        var imageComponentShape = debugImage.transform.GetChild(0).GetComponent<Image>();
        var imageComponent = debugImage.transform.GetChild(1).GetComponent<Image>();
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
        
        Debug.Log($"Completed transfer with {productInTreatment}");
        
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
    
    public void AddDependency(MachineLink link)
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
    }

    public bool CompareLinks(ILinkable start,ILinkable end)
    {
        return (StartLinkable == start && EndLinkable == end);
    }


    #region Event Methods

    private void OnTriggerEnter(Collider other)
    {
        lineInCollision = other.GetComponent<DrawMagicLine>();
        if (lineInCollision is null) return;
        lineInCollision.myLR.material = lineInCollision.linkMaterials[1];
        Debug.LogWarning("Collision avec un autre lien magique");
        lineInCollision.isLinkable = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (lineInCollision is null) return;
        lineInCollision.myLR.material = lineInCollision.linkMaterials[0];
        Debug.LogWarning("Sortie de collision avec un autre lien magique");
        lineInCollision.isLinkable = true;
        lineInCollision = null;
    }

    private void OnCollisionEnter(Collision other)
    {
        Destroy(other.gameObject);
    }

    
    #endregion
}
