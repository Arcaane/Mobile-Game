using UnityEngine;

public class MagicLinesData : MonoBehaviour
{
    [field:Header("Components")]
    [field: SerializeField] public RectTransform buttonTr { get; private set; }
    [field: SerializeField] public Animator deleteButtonAnimator { get; private set; }
    [field: SerializeField] public Transform CollisionPlane { get; private set; }

    [field: Header("Settings")]
    [field: SerializeField] public float slowedTime { get; private set; } = 0.5f; 
    
    [field: Header("Variables")]
    [field: SerializeField] public LayerMask linkableMask{ get; private set; }
    [field: SerializeField] public LayerMask linkLayerMask{ get; private set; }
    [field: SerializeField] public LayerMask floorLayerMask{ get; private set; }
    [field: SerializeField] public Link linkPrefab{ get; private set; }
    public Material[] shaderDarkness;

    private static readonly int IsOpen = Animator.StringToHash("IsOpen");
    private bool areScissorsOpened = false;

    public void OpenScissors(bool value)
    {
        if(areScissorsOpened && value) return;
        areScissorsOpened = value;
    }
}