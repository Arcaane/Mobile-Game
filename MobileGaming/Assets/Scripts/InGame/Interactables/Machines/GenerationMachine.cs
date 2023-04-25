using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GenerationMachine : Machine
{
    [HideInInspector] public Product newProduct;
    [SerializeField] private float timeUntilRefresh = 2;
    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer < timeUntilRefresh) return;
        timer = 0;
        InvokeEndWork();
    }

    public override void StartFeedback()
    {
        if(feedbackText != null) feedbackText.text = $"{newProduct}";
    }

    public override bool IsValidInputProduct(Product product)
    {
        return true;
    }

    protected override void Work()
    {
        
    }

    protected override void PrePing()
    {
        currentProduct = newProduct;
        Debug.Log($"current product : {currentProduct}");
    }

    public override void UnloadProduct(out Product product)
    {
        product = newProduct;
    }
    

#if UNITY_EDITOR
    [CustomEditor(typeof(GenerationMachine)),CanEditMultipleObjects]
    public class GenerationMachineProductEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var machine = (GenerationMachine)target;
            
            EditorGUILayout.LabelField("Work Settings",EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Generated Product :",GUILayout.MaxWidth(160));
            machine.newProduct.data.Shape = (ProductShape) EditorGUILayout.EnumPopup( machine.newProduct.data.Shape);
            machine.newProduct.data.Color = (ProductColor) EditorGUILayout.EnumPopup( machine.newProduct.data.Color);
            
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}

