using System.Collections;
using UnityEngine;

public class DrawMagicLine : MonoBehaviour
{
    [Tooltip("Layer to avoid collision")]
    [SerializeField] public LayerMask linkLayer;
    [Tooltip("Two materials to show if the line collide or not with others lines. \n 0 = Not collide / 1 = Collide")]
    [SerializeField] public Material[] linkMaterials;
    [Tooltip("Time to wait between 2 reset of baking Mesh Collider")] 
    [SerializeField] private float resetCollisionDetectionTime = 0.25f;
    [SerializeField] public bool isLinkable;
    
    private MeshCollider meshCollider;
    [HideInInspector] public LineRenderer myLR;
    
    // Start is called before the first frame update
    void Start()
    {
        linkLayer = LayerMask.NameToLayer("Link");
        myLR = GetComponent<LineRenderer>();
        myLR.material = linkMaterials[0];
        StartCoroutine(Reset(resetCollisionDetectionTime));
        isLinkable = true;
    }
    
    IEnumerator Reset(float _resetTime)
    {
        myLR.Simplify(0.05f);
        if (myLR.positionCount > 3) GenerateMeshCollider();
        
        yield return new WaitForSeconds(_resetTime);
        StartCoroutine(Reset(_resetTime));
    }
     
    private void GenerateMeshCollider()
    {
        meshCollider ??= gameObject.AddComponent<MeshCollider>();
        
        Mesh mesh = new Mesh();
        mesh.name = $"Line";
        myLR.BakeMesh(mesh, true);
        
        meshCollider.sharedMesh = mesh;
    }
}
