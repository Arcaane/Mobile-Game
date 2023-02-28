using UnityEngine;

public class WorkMachine : Machine
{
    [Header("Work Settings")]
    public bool changeColor;
    public ProductColor targetColor;
    public bool changeShape;
    public ProductShape targetShape;

    public override void StartFeedback()
    {
        feedbackText.text = $"{(changeColor ? targetColor : string.Empty)}{(changeShape ? targetShape : string.Empty)}";
    }

    protected override void Work()
    {
        if (changeColor) currentProduct.data.Color = targetColor;
        if (changeShape) currentProduct.data.Shape = targetShape;
    }
}
