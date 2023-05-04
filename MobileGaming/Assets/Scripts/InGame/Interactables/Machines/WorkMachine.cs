using System;
using System.Collections.Generic;
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
    [HideInInspector] public bool changeTopping;
    [SerializeField] private ProductTopping targetTopping;
    
    private List<Action> queueOutputActions = new List<Action>();

    protected override void Setup()
    {
        queueOutputActions.Clear();
        
        
    }

    protected override void Work()
    {
        if (changeColor) currentProduct.data.Color = targetColor;
        if (changeShape) currentProduct.data.Shape = targetShape;
        if (changeTopping) currentProduct.data.Topping = targetTopping;
    }

    public override void AddLinkAction(MachineLink link, Action action)
    {
        queueOutputActions.Add(action);

        link.OnDestroyed += RemoveAction;
        
        void RemoveAction()
        {
            if (queueOutputActions.Contains(action)) queueOutputActions.Remove(action);
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(WorkMachine)),CanEditMultipleObjects]
    public class WorkMachineProductEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var machine = (WorkMachine)target;

            var boolWidth = 100;
            var enumWidth = 100;
            
            EditorGUILayout.LabelField("Work Settings",EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Generated Product :",GUILayout.MaxWidth(160));
            machine.changeColor = EditorGUILayout.ToggleLeft("Change Color",machine.changeColor,GUILayout.MaxWidth(boolWidth));
            machine.changeShape = EditorGUILayout.ToggleLeft("Change Shape",machine.changeShape,GUILayout.MaxWidth(boolWidth));
            machine.changeTopping = EditorGUILayout.ToggleLeft("Change Topping",machine.changeTopping,GUILayout.MaxWidth(boolWidth));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("                   ",GUILayout.MaxWidth(160));
            if(machine.changeColor) machine.targetColor = (ProductColor) EditorGUILayout.EnumPopup(machine.targetColor);
            if(machine.changeShape) machine.targetShape = (ProductShape) EditorGUILayout.EnumPopup(machine.targetShape);
            if(machine.changeTopping) machine.targetTopping = (ProductTopping) EditorGUILayout.EnumPopup(machine.targetTopping);
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}
