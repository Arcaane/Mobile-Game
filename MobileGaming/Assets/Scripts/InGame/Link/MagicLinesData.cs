using UnityEngine;

public class MagicLinesData : MonoBehaviour
{
    [field:Header("Components")]
    [field: SerializeField] public RectTransform buttonTr { get; private set; }
    [field: SerializeField] public Transform CollisionPlane { get; private set; }

    [field: Header("Settings")]
    [field: SerializeField] public float slowedTime { get; private set; } = 0.5f; 
    
    [field: Header("Variables")]
    [field: SerializeField] public LayerMask machineLayerMask{ get; private set; }
    [field: SerializeField] public Link linkPrefab{ get; private set; }
    public Material[] shaderDarkness;
}