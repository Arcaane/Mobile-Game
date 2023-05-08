using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MachineLink : MonoBehaviour
{
    #region Variables
    
    //public TextMeshProUGUI debugPercentageText;
    [FormerlySerializedAs("debugImage")] public Transform bottleImage;
    private DrawMagicLine lineInCollision;
    public Material myMaterial;

    public ILinkable StartLinkable { get; private set; }
    public ILinkable EndLinkable { get; private set; }
    
    // Magic Transportation
    [Range(0,100)] [SerializeField] private int itemProgression = 0;
    public float timeToCompleteTransportation = 5f;
    public float currentTimer = 0f;
    public TextMeshProUGUI lineGroupNumberText;
    public int lineGroupNumber;
    public Product productInTreatment { get; private set; }
    private List<MachineLink> dependentLinks = new List<MachineLink>();
    #endregion

    private void Start()
    {
        var myMaterial = GetComponent<LineRenderer>();
        
        dependentLinks.Clear();
        lineGroupNumberText.text = lineGroupNumber.ToString();
        lineGroupNumberText.gameObject.transform.position = (startLinkable.tr.position + endLinkable.tr.position) / 2 + Vector3.up * 2;
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
        var imageComponentShape = bottleImage.transform.GetChild(0).GetComponent<Image>();
        var imageComponent = bottleImage.transform.GetChild(1).GetComponent<Image>();
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
        
        bottleImage.position = Vector3.Lerp(startLinkable.tr.position + Vector3.up, 
            endLinkable.tr.position + Vector3.up, currentTimer / timeToCompleteTransportation);
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
