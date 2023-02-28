using UnityEngine;

public class GenerationMachine : Machine
{
    [Header("Work Settings")]
    public Product newProduct;

    public override void StartFeedback()
    {
        feedbackText.text = $"{newProduct}";
    }

    protected override void Work()
    {
        
    }

    public override void UnloadProduct(out Product product)
    {
        product = newProduct;
    }
}
