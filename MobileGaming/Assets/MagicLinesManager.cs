using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MagicLinesManager : MonoBehaviour
{
    #region Variables

    [Header("Settings")]
    [SerializeField] private float slowedTime = 0.5f;
    
    [Header("Variables")]
    [SerializeField] private LayerMask machineLayerMask;
    [SerializeField] private bool isInMagicMode;
    
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Vector3[] points;
    [SerializeField] private List<MachineLink> magicLinks;

    // Private
    private bool isPressed;
    private Ray ray;
    private RaycastHit hit;
    private LayerMask linkLayer;
    
    private bool isDraging => InputService.deltaPosition.x != 0 && InputService.deltaPosition.y != 0;

    public Camera orthoCam;
    public Camera perspCam;

    public GameObject currentLineInDrawning;

    private List<ILinkable> currentLinkables = new List<ILinkable>();

    #endregion
    
    private void Start()
    {
        isInMagicMode = false;
        linkLayer = LayerMask.NameToLayer("Link");
        
        ToggleMagic();
    }
    
    private void Update()
    {
        if (!isDraging && !isInMagicMode) return;
        
        if (isDraging) GetClickMachine(InputService.cursorPosition);
        
        if (Input.GetMouseButtonDown(0))
        {
            StartLine();
        }

        if (Input.GetMouseButtonUp(0))
        {
            FinishLine();
        }
    }

    public void SetCameras(Camera cam1, Camera cam2)
    {
        perspCam = cam1;
        orthoCam = cam2;
        orthoCam.gameObject.SetActive(false);
        
        EnableMagicMode();
    }

    private void ToggleMagic()
    {
        isInMagicMode = !isInMagicMode;
        
        if (isInMagicMode) EnableMagicMode();
        else DisableMagicMode();
    }
    
    private void EnableMagicMode()
    {
        // Inputs 
        InputService.OnPress += OnScreenTouch;
        InputService.OnRelease += OnScreenRelease;

        // Camera
        perspCam.gameObject.SetActive(false);
        orthoCam.gameObject.SetActive(true);
    }

    private void DisableMagicMode()
    {
        // Inputs
        InputService.OnPress -= OnScreenTouch;
        InputService.OnRelease -= OnScreenRelease;

        // Camera
        orthoCam.gameObject.SetActive(false);
        perspCam.gameObject.SetActive(true);

        if(drawing != null) StopCoroutine(drawing);
        
        if (currentLineInDrawning != null) Destroy(currentLineInDrawning);
    }

    #region Drag & Drop

    private void OnScreenTouch(Vector2 obj)
    {
        // if (currentMana < 1) return;
        // Sfx can't interact
        // Vfx can't interact

        isPressed = true;
    }

    private void OnScreenRelease(Vector2 obj)
    {
        isPressed = false;
        LinkMachines();
        
        currentLinkables.Clear();
    }
    
    private void LinkMachines()
    {
        Debug.Log($"Linking {currentLinkables.Count} machines");

        CreateMagicLines();
    }

    private void CreateMagicLines()
    {
        for (var index = currentLinkables.Count - 2; index >= 0; index--)
        {
            LinkWithIndex(index,index+1);
        }

        void LinkWithIndex(int index1,int index2)
        {
            var startLinkable = currentLinkables[index1];
            var endLinkable = currentLinkables[index2];
            
            if(!startLinkable.Outputable || !endLinkable.Inputable) return;
            
            if(magicLinks.Any(link => link.CompareLinks(startLinkable,endLinkable))) return;
            
            var magicLineGo = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            
            var lr = magicLineGo.GetComponent<LineRenderer>();
            var machineLink = lr.GetComponent<MachineLink>();
            
            magicLinks.Add(machineLink);

            machineLink.SetLinks(startLinkable,endLinkable);

            machineLink.OnDestroyed += RemoveMachine;
            
            var startLinkablePos = startLinkable.tr.position;
            var endLinkablePos = endLinkable.tr.position;
            var pos1 = startLinkablePos + (endLinkablePos - startLinkablePos).normalized * 0.7f;
            var pos2 = endLinkablePos + (startLinkablePos - endLinkablePos).normalized * 0.7f;

            var start = new Vector3(pos1.x, .5f, pos1.z);
            var end = new Vector3(pos2.x, .5f, pos2.z);
            
            var hits = Physics.RaycastAll(start, end - start, Vector3.Distance(start, end), linkLayer);

            if (hits.Length > 0)
            {
                foreach (var t in hits)
                {
                    var hitLink = t.transform.GetComponent<MachineLink>();
                    if (hitLink == null) continue;
                    
                    machineLink.AddDependency(hitLink);
                    machineLink.enabled = false;
                }
            }
            
            points = new[] { pos1, pos2 };
            lr.positionCount = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                lr.SetPosition(i, points[i] + Vector3.up);
            }

            GenerateLinkCollider(lr, p1, p2);

            void RemoveMachine()
            {
                if (magicLinks.Contains(machineLink)) magicLinks.Remove(machineLink);
            }
        }
    }

    private Machine GetClickMachine(Vector2 mousePos)
    {
        Ray ray = orthoCam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out hit, machineLayerMask))
        {
            var linkable = hit.transform.GetComponent<ILinkable>();
            
            if (linkable == null) return null;
            
            if(!currentLinkables.Contains(linkable)){ currentLinkables.Add(linkable);}

            return null;
        }

        return null;
    }

    #endregion

    #region DrawLines&Mesh

    private Vector3 p1;
    private Vector3 p2;
    private void GenerateLinkCollider(LineRenderer lineRenderer, Vector3 p1, Vector3 p2)
    {
        lineRenderer.gameObject.transform.forward = (p2 - p1).normalized;
        Mesh mesh = new Mesh();
        
        lineRenderer.BakeMesh(mesh, true);
        lineRenderer.gameObject.layer = LayerMask.NameToLayer("Link");
        lineRenderer.gameObject.transform.position = (p1 + p2) / 2;
        
        BoxCollider linkCollider;
        linkCollider = lineRenderer.gameObject.AddComponent<BoxCollider>();
        linkCollider.center = new Vector3(0,1,0);
        linkCollider.size = new Vector3(.5f, .5f, Vector3.Distance(p2, p1));
        linkCollider.isTrigger = true;
    }
    
    private Coroutine drawing;
    
    private void StartLine()
    {
        if (drawing != null)
        {
            StopCoroutine(drawing);
        }

        
        drawing = StartCoroutine(DrawLine());
    }

    public void FinishLine()
    {
        if (drawing == null) return;
        StopCoroutine(drawing);
        Destroy(currentLineInDrawning);
        currentLineInDrawning = null;
    }

    IEnumerator DrawLine()
    {
        currentLineInDrawning = Instantiate(Resources.Load("Line") as GameObject,
            new Vector3(0, 0, 0), Quaternion.identity);
        LineRenderer line = currentLineInDrawning.GetComponent<LineRenderer>();
        line.positionCount = 0;

        while (true)
        {
            Vector3 position = orthoCam.ScreenToWorldPoint(Input.mousePosition);
            position.y = .5f;
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, position);
            yield return null;
        }
    }
    
    #endregion
}