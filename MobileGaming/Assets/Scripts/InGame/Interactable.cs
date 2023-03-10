using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract void Interact(Product inProduct,out Product outProduct);

    public void EnterRange()
    {
        OnRangeEnter?.Invoke(this);
    }

    public void ExitRange()
    {
        OnRangeExit?.Invoke(this);
    }
    
    public static void ResetEvents()
    {
        OnRangeEnter = null;
        OnRangeExit = null;
    }
    public static event Action<Interactable> OnRangeEnter;
    public static event Action<Interactable> OnRangeExit;
}
