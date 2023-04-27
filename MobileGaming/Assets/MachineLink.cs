using System;
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
    private ILinkable startLinkable;
    private ILinkable endLinkable;
    
    // Magic Transportation
    private Product productInTreatment;
    [Range(0,100)] [SerializeField] private int itemProgression = 0;
    public float timeToCompleteTransportation = 5f;
    public float currentTimer = 0f;
    public TextMeshProUGUI lineGroupNumberText;
    public int lineGroupNumber;
    private List<MachineLink> dependentLinks = new List<MachineLink>();
    #endregion

    private void Start()
    {
        var myMaterial = GetComponent<LineRenderer>();
        
        dependentLinks.Clear();
        lineGroupNumberText.text = lineGroupNumber.ToString();
        lineGroupNumberText.gameObject.transform.position = (startLinkable.tr.position + endLinkable.tr.position) / 2 + Vector3.up * 2;
    }

    private void MoveProduct()
    {
        startLinkable.Output(out productInTreatment);
        
        if(productInTreatment == null) return;
        
        startLinkable.OnOutput -= TryInputOutput;
        
        currentTimer = 0f;
        
        SetUIProduct();
    }
    
    private void Update()
    {
        if(productInTreatment == null) return;
        
        currentTimer += Time.deltaTime;
        if (currentTimer > timeToCompleteTransportation)
        {
            CompleteTransfer();
        }
        
        Feedback();
    }

    private void CompleteTransfer()
    {
        currentTimer = 0;
        
        endLinkable.Input(productInTreatment);
            
        productInTreatment = null;
        
        OnDestroyed?.Invoke();
        
        Destroy(gameObject);
    }

    public void SetLinks(ILinkable startLink,ILinkable endLink)
    {
        if(!startLink.Outputable || !endLink.Inputable) return;
        
        startLinkable = startLink;
        endLinkable = endLink;
        
        startLinkable.OnOutput += TryInputOutput;
        
        startLinkable.Ping();
    }
    
    void TryInputOutput(Product outProduct)
    {
        MoveProduct();
    }

    public void AddDependency(MachineLink machineLink)
    {
        if(dependentLinks.Contains(machineLink)) return;
        
        dependentLinks.Add(machineLink);
        
        machineLink.OnDestroyed += RemoveDependency;
        
        void RemoveDependency()
        {
            if(!dependentLinks.Contains(machineLink)) return;
            dependentLinks.Remove(machineLink);
            if(dependentLinks.Count > 0) return;
            enabled = true;
        }
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
    
    public event Action OnDestroyed;
    
    private void Feedback()
    {
        itemProgression = (int)((currentTimer / timeToCompleteTransportation) * 100);
        //myMaterial.SetFloat(MaterialInnerColor, 1 - currentTimer / timeToCompleteTransportation);
        
        bottleImage.position = Vector3.Lerp(startLinkable.tr.position + Vector3.up, 
            endLinkable.tr.position + Vector3.up, currentTimer / timeToCompleteTransportation);
        
    }

    public bool CompareLinks(ILinkable start,ILinkable end)
    {
        return (startLinkable == start && endLinkable == end);
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
