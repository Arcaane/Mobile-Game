using System.Collections;
using UnityEngine;

public class DrawMagicLine : MonoBehaviour
{
    [Tooltip("Two materials to show if the line collide or not with others lines. \n 0 = Not collide / 1 = Collide")]
    [SerializeField] public Material[] linkMaterials;
    [Tooltip("Time to wait between 2 reset of baking Mesh Collider")] 
    [SerializeField] private float resetCollisionDetectionTime = 0.25f;

    private MeshCollider meshCollider;
    [HideInInspector] public LineRenderer myLR;
    
    
    void Start()
    {
        myLR = GetComponent<LineRenderer>();
        myLR.material = linkMaterials[0];
        StartCoroutine(Reset(resetCollisionDetectionTime));
    }
    
    IEnumerator Reset(float _resetTime)
    {
        myLR.Simplify(0.05f);
        yield return new WaitForSeconds(_resetTime);
        StartCoroutine(Reset(_resetTime));
    }
}
