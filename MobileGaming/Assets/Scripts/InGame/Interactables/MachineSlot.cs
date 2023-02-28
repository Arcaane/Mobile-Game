using System;
using Unity.VisualScripting;
using UnityEngine;

public class MachineSlot : Interactable
{
    [field:SerializeField] public Machine machine { get; private set; }
    
    public void SetMachine(Machine newMachine)
    {
        machine = newMachine;
    }

    public override void Interact(Product inProduct,out Product outProduct)
    {
        if (machine is null)
        {
            outProduct = inProduct;
            return;
        }
        
        machine.LoadProduct(inProduct,out outProduct);
        return;
        
        if (inProduct is not null)
        {
            machine.LoadProduct(inProduct);
        }
        machine.UnloadProduct(out outProduct);
    }

}
