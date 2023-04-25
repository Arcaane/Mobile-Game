using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorkMachine : Machine
{
    [HideInInspector] public bool changeColor;
    [SerializeField] private ProductColor targetColor;
    [HideInInspector] public bool changeShape;
    [SerializeField] private ProductShape targetShape;

    public override void StartFeedback()
    {
        if(feedbackText != null) feedbackText.text = $"{(changeColor ? targetColor : string.Empty)}{(changeShape ? targetShape : string.Empty)}";
    }

    public override bool IsValidInputProduct(Product product)
    {
        if (changeColor && changeShape && product.data.Color == targetColor && product.data.Shape == targetShape) return false;
        if (changeColor && !changeShape && product.data.Color == targetColor) return false;
        if (changeShape && !changeColor && product.data.Shape == targetShape) return false;
        return true;
    }

    protected override void Work()
    {
        if (changeColor) currentProduct.data.Color = targetColor;
        if (changeShape) currentProduct.data.Shape = targetShape;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WorkMachine)),CanEditMultipleObjects]
    public class WorkMachineProductEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var machine = (WorkMachine)target;
            
            EditorGUILayout.LabelField("Work Settings",EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Generated Product :",GUILayout.MaxWidth(160));
            machine.changeColor = EditorGUILayout.ToggleLeft("Change Color",machine.changeColor,GUILayout.MinWidth(120));
            machine.changeShape = EditorGUILayout.ToggleLeft("Change Shape",machine.changeShape);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("                   ",GUILayout.MaxWidth(160));
            if(machine.changeColor) machine.targetColor = (ProductColor) EditorGUILayout.EnumPopup(machine.targetColor,GUILayout.MinWidth(10));
            if(machine.changeShape) machine.targetShape = (ProductShape) EditorGUILayout.EnumPopup(machine.targetShape);
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}
