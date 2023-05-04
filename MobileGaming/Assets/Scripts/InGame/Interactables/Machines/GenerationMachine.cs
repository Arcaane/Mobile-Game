using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GenerationMachine : Machine
{
    [HideInInspector] public Product newProduct;
    private float timer;

    public override bool Inputable => false;

    protected override void Setup()
    {
        
    }

    protected override void Work()
    {
        
    }
    

    protected override void UnloadProduct(out Product product)
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
            machine.newProduct.data.Topping = (ProductTopping) EditorGUILayout.EnumPopup( machine.newProduct.data.Topping);
            
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}

