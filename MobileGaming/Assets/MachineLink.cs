using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineLink : MonoBehaviour
{
    #region Variables
    
    //public TextMeshProUGUI debugPercentageText;
    public Transform debugImage;
    private DrawMagicLine lineInCollision;
    public List<Machine> machinesInLinks;
    public Material myMaterial;
    
    // Magic Transportation
    private Product productInTreatment;
    [Range(0,100)] public int itemProgression = 0;
    public float timeToCompleteTransportation = 10f;
    public float currentTimer = 0f;

    public Sprite[] bottleShapesSprites;
    public Sprite[] bottleContentSprites;
    #endregion

    private void Start()
    {
        myMaterial = GetComponent<LineRenderer>().material;
        productInTreatment = machinesInLinks[0].GetInformationOnMachineProduct();
        SetUIProduct();
    }

    
    
    private void SetUIProduct()
    {
        // Forme de la bouteille
        switch (productInTreatment.data.Shape)
        {
            case ProductShape.Hearth: debugImage.transform.GetChild(0).GetComponent<Image>().sprite = bottleShapesSprites[0];
                switch (productInTreatment.data.Color)
                {
                    case ProductColor.Transparent: Debug.Log("Hearh Shape without content"); break;
                    case ProductColor.Blue: debugImage.transform.GetChild(1).GetComponent<Image>().sprite = bottleContentSprites[0]; break;
                    case ProductColor.Green: debugImage.transform.GetChild(1).GetComponent<Image>().sprite = bottleContentSprites[1]; break;
                    case ProductColor.Red: debugImage.transform.GetChild(1).GetComponent<Image>().sprite = bottleContentSprites[2]; break;
                }
                break;
            case ProductShape.Cross: debugImage.transform.GetChild(0).GetComponent<Image>().sprite = bottleShapesSprites[1];
                switch (productInTreatment.data.Color)
                {
                    case ProductColor.Transparent: break;
                    case ProductColor.Blue: debugImage.transform.GetChild(1).GetComponent<Image>().sprite = bottleContentSprites[3]; break;
                    case ProductColor.Green: debugImage.transform.GetChild(1).GetComponent<Image>().sprite = bottleContentSprites[4]; break;
                    case ProductColor.Red: debugImage.transform.GetChild(1).GetComponent<Image>().sprite = bottleContentSprites[5]; break;
                }
                break;
            case ProductShape.Moon: debugImage.transform.GetChild(0).GetComponent<Image>().sprite = bottleShapesSprites[2];
                switch (productInTreatment.data.Color)
                {
                    case ProductColor.Transparent: break;
                    case ProductColor.Blue: debugImage.transform.GetChild(1).GetComponent<Image>().sprite = bottleContentSprites[6]; break;
                    case ProductColor.Green: debugImage.transform.GetChild(1).GetComponent<Image>().sprite = bottleContentSprites[7]; break;
                    case ProductColor.Red: debugImage.transform.GetChild(1).GetComponent<Image>().sprite = bottleContentSprites[8]; break;
                }
                break;
        }
    }

    private void Update()
    {
        if (machinesInLinks[1].currentProduct != null) return; //place dispo a destination
        
        if(productInTreatment == null) return; //place dispo sur moi
        
        currentTimer += Time.deltaTime;
        if (currentTimer > timeToCompleteTransportation)
        {
            DeliverProductIntoMachine();
            currentTimer = 0;
        }
        
        Feedback();
    }

    public void TakeProductFromMachine()
    {
        machinesInLinks[0].UnloadProduct(out productInTreatment);
    }
    
    private void DeliverProductIntoMachine()
    {
        machinesInLinks[1].ReceiveProductFromLink(productInTreatment);
        productInTreatment = null;
    }

    private void Feedback()
    {
        itemProgression =  (int)((currentTimer / timeToCompleteTransportation) * 100);
        myMaterial.SetFloat("_FilingValue", 1 - currentTimer / timeToCompleteTransportation);
        
        debugImage.position = Vector3.Lerp(machinesInLinks[0].transform.position + Vector3.up, 
            machinesInLinks[1].transform.position + Vector3.up, currentTimer / timeToCompleteTransportation);
        
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
