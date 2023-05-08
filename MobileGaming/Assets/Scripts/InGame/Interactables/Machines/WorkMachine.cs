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
    
    private List<Link> nextLinksToLoad = new ();
    [SerializeField] private List<Link> nextLinksToUnload = new ();
    private bool hasProductToLoad => !IsWorking && currentProduct != null;

    protected override void Setup()
    {
        nextLinksToLoad.Clear();
        nextLinksToUnload.Clear();
    }

    protected override void EndWork()
    {
        if (changeColor) currentProduct.data.Color = targetColor;
        if (changeShape) currentProduct.data.Shape = targetShape;
        if (changeTopping) currentProduct.data.Topping = targetTopping;
        
        LoadNextLink();
    }

    public override bool IsAvailable(Link link)
    {
        if (nextLinksToUnload.Count <= 0) return false;
        
        if (IsWorking) return false;
        if (currentProduct != null) return false;
        
        return link == nextLinksToUnload[0];
    }

    private void LoadNextLink()
    {
        if(nextLinksToLoad.Count <= 0) return;
        
        var nextLink = nextLinksToLoad[0];
        nextLinksToLoad.RemoveAt(0);

        if (nextLink.EndLinkable.IsAvailable(nextLink))
        {
            nextLink.LoadProduct(currentProduct);
            currentProduct = null;
            return;
        }

        nextLink.EndLinkable.OnAvailable += LoadProductInLink;

        void LoadProductInLink()
        {
            if(!nextLink.EndLinkable.IsAvailable(nextLink)) return;
            
            nextLink.LoadProduct(currentProduct);
            currentProduct = null;
            nextLink.EndLinkable.OnAvailable -= LoadProductInLink;
        }
    }
    
    public override void SetStartLinkable(Link link)
    {
        nextLinksToLoad.Add(link);
        link.OnDestroyed += RemoveLinkFromList;
        
        if(nextLinksToLoad.Count == 1 && hasProductToLoad) LoadNextLink();

        void RemoveLinkFromList()
        {
            if (nextLinksToLoad.Contains(link)) nextLinksToLoad.Remove(link);
        }
    }
    
    public override void SetEndLinkable(Link link)
    {
        link.OnComplete += StartWork;
        
        nextLinksToUnload.Add(link);
        link.OnDestroyed += RemoveLinkFromList;
        
        void RemoveLinkFromList()
        {
            if (nextLinksToUnload.Contains(link)) nextLinksToUnload.Remove(link);
        }
    }


    #region Editor
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
    #endregion
}
