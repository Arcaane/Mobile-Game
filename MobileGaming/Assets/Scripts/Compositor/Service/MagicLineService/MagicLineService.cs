using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

public class MagicLineService : SwitchableService, IMagicLineService
{
    private MagicLinesData magicLinesData;

    private RectTransform buttonTr;
    private LayerMask machineLayerMask;
    
    private Vector3[] points;
    private List<Link> magicLinks = new List<Link>();
    
    private bool isPressed;
    private RaycastHit hit;
    private LayerMask linkLayer;
    private Link linkToDestroy;
    private bool inDestroyMode;
    private Vector3 buttonPos;
    
    private Coroutine drawing;
    private static readonly int Darkness2 = Shader.PropertyToID("_Darkness2");
    
    private bool isDraging => InputService.deltaPosition.x != 0 && InputService.deltaPosition.y != 0;

    private Camera cam;

    private GameObject currentLineInDrawning;

    private List<ILinkable> currentLinkables = new List<ILinkable>();
    
    public MagicLineService(bool startState) : base(startState)
    {
    }
    
    public void SetData(MagicLinesData data)
    {
        Disable();

        magicLinesData = data;
        
        inDestroyMode = false;

        buttonTr = magicLinesData.buttonTr;
        buttonPos = buttonTr.position;

        machineLayerMask = magicLinesData.machineLayerMask;
        linkLayer = LayerMask.NameToLayer("Link");
    }

    public void SetCamera(Camera camera)
    {
        cam = camera;
        var camPos = cam.transform.position;
        
        var height = 2.0f * Mathf.Tan(0.5f * cam.fieldOfView * Mathf.Deg2Rad) * camPos.y;
        var width = height * Screen.width / Screen.height;

        magicLinesData.CollisionPlane.position = new Vector3(camPos.x, 0, camPos.y);
        magicLinesData.CollisionPlane.localScale = new Vector3(width, height, 1f);
    }


    public override void Enable()
    {
        Debug.Log("Enable Service");
        
        InputService.OnPress += OnScreenTouch;
        InputService.OnRelease += OnScreenRelease;
        
        base.Enable();
    }

    public override void Disable()
    {
        InputService.OnPress -= OnScreenTouch;
        InputService.OnRelease -= OnScreenRelease;
        
        if(drawing != null) magicLinesData.StopCoroutine(drawing);
        
        if (currentLineInDrawning != null) Object.Destroy(currentLineInDrawning);
    }

    [OnTick]
    private void Update()
    {
        if(!enable) return;
        
        if (!isDraging) return;

        if (inDestroyMode)
        {
            buttonTr.position = InputService.cursorPosition;
            DestroySetLink(InputService.cursorPosition);
            return;
        }
        
        if (isDraging)
        {
            GetClickMachine(InputService.cursorPosition);
        }
    }
    
    private void OnScreenTouch(Vector2 obj)
    {
        isPressed = true;
        Time.timeScale = magicLinesData.slowedTime;
        foreach (var t in magicLinesData.shaderDarkness) t.SetFloat(Darkness2, 0.3f);
        
        if(SelectButton()) return;
        
        StartLine();
    }

    private bool SelectButton()
    {
        var pos = InputService.cursorPosition;

        if (buttonTr == null) return false;
        if (pos.x > buttonPos.x + buttonTr.sizeDelta.x * 2) return false;
        if (pos.y > buttonPos.y + buttonTr.sizeDelta.y * 2) return false;
        if (pos.x < buttonPos.x) return false;
        if (pos.y < buttonPos.y) return false;
        
        buttonTr.pivot = Vector2.one * 0.5f;
        inDestroyMode = true;
        return true;
    }

    private void OnScreenRelease(Vector2 obj)
    {
        isPressed = false;
        Time.timeScale = 1;
        
        foreach (var t in magicLinesData.shaderDarkness) t.SetFloat(Darkness2, 1f);
        
        buttonTr.pivot = Vector2.zero;
        inDestroyMode = false;
        buttonTr.position = buttonPos;
        
        if (linkToDestroy != null)
        {
            linkToDestroy.Destroy();
            linkToDestroy = null;
            return;
        }
        
        FinishLine();
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
            
            var link = Object.Instantiate(magicLinesData.linkPrefab, Vector3.zero, Quaternion.identity);
            
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
            
            link.SetPoints(cam);
            
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
        var ray = cam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out hit, machineLayerMask)) return;
        var linkable = hit.transform.GetComponent<ILinkable>();
            
        if (linkable == null) return;
            
        if(!currentLinkables.Contains(linkable)){ currentLinkables.Add(linkable);}
    }

    private void DestroySetLink(Vector2 mousePos)
    {
        var ray = cam.ScreenPointToRay(mousePos);

        linkToDestroy = null;
        if (!Physics.Raycast(ray, out hit, linkLayer)) return;
        var link = hit.transform.GetComponent<Link>();
        
        linkToDestroy = link != null ? link : null;
    }

    #region DrawLines&Mesh
    
    private void StartLine()
    {
        if (drawing != null)
        {
            Object.Destroy(currentLineInDrawning);
            magicLinesData.StopCoroutine(drawing);
        }
        
        drawing = magicLinesData.StartCoroutine(DrawLine());
    }

    private void FinishLine()
    {
        if (drawing == null) return;
        magicLinesData.StopCoroutine(drawing);
        Object.Destroy(currentLineInDrawning);
        currentLineInDrawning = null;
    }

    IEnumerator DrawLine()
    {
        currentLineInDrawning = Object.Instantiate(Resources.Load("Line") as GameObject,
            new Vector3(0, 0, 0), Quaternion.identity);
        LineRenderer line = currentLineInDrawning.GetComponent<LineRenderer>();
        line.positionCount = 0;

        while (true)
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin,ray.direction * 100f,Color.yellow);

            if (!Physics.Raycast(ray, out hit))
            {
                yield return null;
                continue;
            }

            var point = hit.point + ray.direction * (-1 * 2f);
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, point);
            yield return null;
        }
    }
    
    #endregion
}
