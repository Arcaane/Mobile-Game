using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class MagicLinesManager : MonoBehaviour
{
    #region Variables

    [Header("Settings")]
    [SerializeField] private float slowedTime = 0.5f;
    
    [Header("Variables")]
    [SerializeField] private LayerMask machineLayerMask;
    [SerializeField] private LayerMask linkLayerMask;
    [SerializeField] private bool isInMagicMode;
    
    [SerializeField] private Link linkPrefab;
    [SerializeField] private Vector3[] points;
    [SerializeField] private List<Link> magicLinks;

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

        if (isDraging)
        {
            if (Input.GetKey(KeyCode.A))
            {
                Debug.Log("Destruction");
                DestroyClickLink(InputService.cursorPosition);
                return;
            }
            
            GetClickMachine(InputService.cursorPosition);
        }

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

    public Material[] shaderDarkness;
    
    private void OnScreenTouch(Vector2 obj)
    {
        isPressed = true;
        Time.timeScale = slowedTime;
        foreach (var t in shaderDarkness) t.SetInt("_Darkness", 1);
    }

    private void OnScreenRelease(Vector2 obj)
    {
        isPressed = false;
        Time.timeScale = 1;
        
        foreach (var t in shaderDarkness) t.SetInt("_Darkness", 0);
        LinkMachines();
        
        currentLinkables.Clear();
    }
    
    private void LinkMachines()
    {
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
            
            //if(magicLinks.Any(link => /*link.CompareLinks(startLinkable,endLinkable) ||*/ link.CompareLinks(endLinkable,startLinkable))) return;
            
            var link = Instantiate(linkPrefab, Vector3.zero, Quaternion.identity);
            
            var lr = link.LineRenderer;

            magicLinks.Add(link);

            link.SetLinks(startLinkable,endLinkable);

            link.OnDestroyed += RemoveMachine;
            
            var startLinkablePos = startLinkable.Position;
            var endLinkablePos = endLinkable.Position;
            var pos1 = startLinkablePos + (endLinkablePos - startLinkablePos).normalized * 0.7f;
            var pos2 = endLinkablePos + (startLinkablePos - endLinkablePos).normalized * 0.7f;

            var start = new Vector3(pos1.x, .5f, pos1.z);
            var end = new Vector3(pos2.x, .5f, pos2.z);
            
            var hits = Physics.RaycastAll(start, end - start, Vector3.Distance(start, end), linkLayer);

            if (hits.Length > 0)
            {
                foreach (var t in hits)
                {
                    var hitLink = t.transform.GetComponent<Link>();
                    if (hitLink == null) continue;
                    
                    link.AddDependency(hitLink);
                    link.enabled = false;
                }
            }
            
            points = new[] { pos1, pos2 };
            lr.positionCount = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                lr.SetPosition(i, points[i] + Vector3.up);
            }
            
            link.SetPoints();
            
            CheckLinkCollisions(link);
            
            void RemoveMachine()
            {
                if (magicLinks.Contains(link)) magicLinks.Remove(link);
            }
        }
    }

    private void CheckLinkCollisions(Link createdLink)
    {
        var intersectingLinks = new List<Link>();
        
        foreach (var link in magicLinks.Where(link => link != createdLink))
        {
            for (int i = 1; i < createdLink.Points.Length; i++)
            {
                var p = createdLink.Points[i - 1];
                var q = createdLink.Points[i];
                if (IntersectWithArray(p, q, link.Points)) intersectingLinks.Add(link);
            }
        }
        
        bool IntersectWithArray(Vector2 p,Vector2 q,Vector2[] array)
        {
            if (array.Length < 1) return false;
            for (int i = 1; i < array.Length; i++)
            {
                if (Intersect(p, q, array[i - 1], array[i])) return true;
            }
            return false;
        }

        // Given three collinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
        {
            return q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
                   q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y);
        }

        int Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            var val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
  
            if (val == 0) return 0; // collinear
  
            return (val > 0)? 1: 2; // clock or counterclock wise
        }

        bool Intersect(Vector2 p1,Vector2 q1, Vector2 p2, Vector2 q2)
        {
            var o1 = Orientation(p1, q1, p2);
            var o2 = Orientation(p1, q1, q2);
            var o3 = Orientation(p2, q2, p1);
            var o4 = Orientation(p2, q2, q1);
  
            // General case
            if (o1 != o2 && o3 != o4) return true;
  
            // Special Cases
            // p1, q1 and p2 are collinear and p2 lies on segment p1q1
            if (o1 == 0 && OnSegment(p1, p2, q1)) return true;
  
            // p1, q1 and q2 are collinear and q2 lies on segment p1q1
            if (o2 == 0 && OnSegment(p1, q2, q1)) return true;
  
            // p2, q2 and p1 are collinear and p1 lies on segment p2q2
            if (o3 == 0 && OnSegment(p2, p1, q2)) return true;
  
            // p2, q2 and q1 are collinear and q1 lies on segment p2q2
            if (o4 == 0 && OnSegment(p2, q1, q2)) return true;
  
            return false; // Doesn't fall in any of the above cases
        }
    }

    private void GetClickMachine(Vector2 mousePos)
    {
        var ray = orthoCam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out hit, machineLayerMask)) return;
        var linkable = hit.transform.GetComponent<ILinkable>();
            
        if (linkable == null) return;
            
        if(!currentLinkables.Contains(linkable)){ currentLinkables.Add(linkable);}
    }

    private void DestroyClickLink(Vector2 mousePos)
    {
        var ray = orthoCam.ScreenPointToRay(mousePos);
        
        Debug.DrawRay(ray.origin,ray.direction,Color.yellow);
        
        if (!Physics.Raycast(ray, out hit, linkLayer)) return;
        var link = hit.transform.GetComponent<Link>();
        
        if (link == null) return;
        
        link.Destroy();
    }

    #endregion

    #region DrawLines&Mesh
    
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