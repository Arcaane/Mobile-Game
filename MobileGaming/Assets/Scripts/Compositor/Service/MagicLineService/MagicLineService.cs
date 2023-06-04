using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

public class MagicLineService : SwitchableService, IMagicLineService
{
    private MagicLinesData magicLinesData;

    private RectTransform scissorsButtonTr;
    private LayerMask linkableMask;

    private Vector3[] points;
    private List<Link> magicLinks = new List<Link>();

    private bool isPressed;
    private RaycastHit hit;
    private LayerMask linkLayer;
    private LayerMask floorLayer;
    private Link linkToDestroy;
    private bool canDestroy = true;
    private bool inDestroyMode;
    private Vector3 buttonPos;
    private Coroutine drawing;
    private bool InDestroyMode => canDestroy && inDestroyMode;
    private bool isDraging => InputService.deltaPosition.x != 0 && InputService.deltaPosition.y != 0;

    private Camera cam;

    private GameObject currentLineInDrawning;

    private ILinkable lastCheckedLinkable;
    private List<ILinkable> currentLinkables = new List<ILinkable>();
    
    public MagicLineService(bool startState) : base(startState)
    {
    }

    [ServiceInit]
    private void SetListeners()
    {
        EventManager.AddListener<LoadLevelEvent>(SetCamera);
        EventManager.AddListener<LoadLevelEvent>(CleanupListeners);
        EventManager.AddListener<LoadTutorialEvent>(DeactivateLinkDestructionInTutorial);

        void CleanupListeners(LoadLevelEvent loadLevelEvent)
        {
            CanDestroyLinks(true);
            
            EventManager.RemoveListeners<ActivateDestroyModeEvent>();
        }
        
        void SetCamera(LoadLevelEvent loadLevelEvent)
        {
            cam = loadLevelEvent.Level.Camera;
            
            var camPos = cam.transform.position;
        
            var height = 2.0f * Mathf.Tan(0.5f * cam.fieldOfView * Mathf.Deg2Rad) * camPos.y;
            var width = height * Screen.width / Screen.height;
            
            magicLinesData.CollisionPlane.position = new Vector3(camPos.x, 0, camPos.z);
            magicLinesData.CollisionPlane.localScale = new Vector3(width, height, 1f);
            
            //Debug.Log($"Setting Camera, position : {camPos}, plane position : {magicLinesData.CollisionPlane.position}");
        }
    }
    
    public void SetData(MagicLinesData data)
    {
        magicLinesData = data;

        canDestroy = true;
        inDestroyMode = false;

        scissorsButtonTr = magicLinesData.buttonTr;
        buttonPos = scissorsButtonTr.position;
        
        linkableMask = magicLinesData.linkableMask;
        linkLayer = magicLinesData.linkLayerMask;
        floorLayer = magicLinesData.floorLayerMask;
        
        scissorsButtonTr.gameObject.SetActive(false);
    }
    
    public override void Enable()
    {
        InputService.OnPress += OnScreenTouch;
        InputService.OnRelease += OnScreenRelease;
        
        scissorsButtonTr.gameObject.SetActive(canDestroy);
        
        base.Enable();
    }

    public override void Disable()
    {
        InputService.OnPress -= OnScreenTouch;
        InputService.OnRelease -= OnScreenRelease;
        
        scissorsButtonTr.gameObject.SetActive(false);
        
        base.Disable();
        
        if(drawing != null) magicLinesData.StopCoroutine(drawing);
        
        if (currentLineInDrawning != null) Object.Destroy(currentLineInDrawning);
    }
    
    private void OnScreenTouch(Vector2 obj)
    {
        if(!enable) return;
        
        isPressed = true;
        Time.timeScale = magicLinesData.slowedTime;
        
        EventManager.Trigger(new ActivateDarkmodeEvent(true));

        SelectButton();
        
        StartLine();
    }

    private void SelectButton()
    {
        if(!canDestroy) return;
        
        var pos = InputService.cursorPosition;

        if (scissorsButtonTr == null) return;
        if (pos.x > buttonPos.x + scissorsButtonTr.sizeDelta.x * 2) return;
        if (pos.y > buttonPos.y + scissorsButtonTr.sizeDelta.y * 2) return;
        if (pos.x < buttonPos.x) return;
        if (pos.y < buttonPos.y) return;
        
        scissorsButtonTr.pivot = Vector2.one * 0.5f;
        inDestroyMode = true;
        EventManager.Trigger(new ActivateDestroyModeEvent(true));
    }

    private void OnScreenRelease(Vector2 obj)
    {
        isPressed = false;
        Time.timeScale = 1;
        
        EventManager.Trigger(new ActivateDarkmodeEvent(false));
        
        scissorsButtonTr.pivot = Vector2.zero;
        if (inDestroyMode)
        {
            inDestroyMode = false;
            EventManager.Trigger(new ActivateDestroyModeEvent(false));
        }
        scissorsButtonTr.position = buttonPos;
        
        foreach (var linkable in currentLinkables)
        {
            linkable.ShowHighlight(false);
        }
        
        if (linkToDestroy != null)
        {
            magicLinesData.OpenScissors(false);
            linkToDestroy.DestroyLink();
            linkToDestroy = null;
            return;
        }
        
        FinishLine();
        LinkMachines();
        
        currentLinkables.Clear();
        lastCheckedLinkable = null;
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
            
            //if(magicLinks.Any(link => /*link.CompareLinks(startLinkable,endLinkable) ||*/ link.CompareLinks(endLinkable,startLinkable))) return;
            
            CreateLink(startLinkable,endLinkable);
        }
    }

    public void CreateLink(ILinkable startLinkable,ILinkable endLinkable)
    {
        if(!startLinkable.Outputable || !endLinkable.Inputable) return;
        
        var link = Object.Instantiate(magicLinesData.linkPrefab, Vector3.zero, Quaternion.identity);
            
        var lr = link.LineRenderer;
            
        link.OnDestroyed += RemoveLinkFromList;
        magicLinks.Add(link);

        var totalLinks = currentLinkables.Count - 1;
        if (totalLinks < 0) totalLinks = 1; 
        link.SetLinks(startLinkable,endLinkable,totalLinks);
        if(link.FlaggedForDestruction) return;
        
        var startLinkablePos = startLinkable.Position;
        var endLinkablePos = endLinkable.Position;

        var start = new Vector3(startLinkablePos.x, 0, startLinkablePos.z);
        var end = new Vector3(endLinkablePos.x, 0, endLinkablePos.z);
            
        var hits = Physics.RaycastAll(start, end - start, Vector3.Distance(start, end), linkLayer);

        if (hits.Length > 0)
        {
            var collidingLinks = hits.Where(h => h.transform.GetComponent<Link>() != null)
                .Select(h => h.transform.GetComponent<Link>()).ToArray();
            if(collidingLinks.Length > 0) EventManager.Trigger(new LinkCollisionEvent(link,collidingLinks));
        }

        start.y = 0.5f;
        end.y = 0.5f;
        points = new[] { start, end };
        lr.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            lr.SetPosition(i, points[i]);
        }
            
        link.CreateMesh();
        
        void RemoveLinkFromList()
        {
            if (magicLinks.Contains(link)) magicLinks.Remove(link);
        }
    }
    
    private void DeactivateLinkDestructionInTutorial(LoadTutorialEvent _)
    {
        CanDestroyLinks(false);
    }

    public void CanDestroyLinks(bool value)
    {
        canDestroy = value;
        scissorsButtonTr.gameObject.SetActive(canDestroy);
    }
    
    private void GetLinkable(Vector3 position)
    {
        position -= Vector3.up;
        if (!Physics.Raycast(position, Vector3.up, out hit, 3f, linkableMask))
        {
            //Debug.DrawRay(position,Vector3.up*3f,Color.red);
            return;
        }
        
        //Debug.DrawRay(position,Vector3.up*3f,Color.green);
        var linkable = hit.transform.GetComponent<ILinkable>();
        if (linkable == null) return;

        if(linkable == lastCheckedLinkable) return;
        lastCheckedLinkable = linkable;
        
        if(!CanBeLinked(linkable)) return;
        
        if (currentLinkables.Contains(linkable)) return;
        
        linkable.ShowHighlight(true);
        currentLinkables.Add(linkable);

        bool CanBeLinked(ILinkable newLinkable)
        {
            if (!newLinkable.Inputable && !newLinkable.Outputable) return false;
            
            if (!newLinkable.Inputable && currentLinkables.Count > 0) return false;
            
            var otherClient = currentLinkables.FirstOrDefault(other => !other.Outputable);

            if (otherClient == null) return true;
            
            otherClient.ShowHighlight(false);
            currentLinkables.Remove(otherClient);
            return true;
        }
    }
    
    private void GetLinkToDestroy(Vector3 position)
    {
        position -= Vector3.up;
        if (!Physics.Raycast(position, Vector3.up, out hit, 3f, linkLayer))
        {
            //Debug.DrawRay(position,Vector3.up*3f,Color.red);
            magicLinesData.OpenScissors(false);
            linkToDestroy = null;
            return;
        }
        
        //Debug.DrawRay(position,Vector3.up*3f,Color.green);
        linkToDestroy = null;
        var link = hit.transform.GetComponent<Link>();

        linkToDestroy = link != null ? link : null;
        magicLinesData.OpenScissors(true);
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
        var line = currentLineInDrawning.GetComponent<LineRenderer>();
        line.positionCount = 0;

        while (isPressed)
        {
            var ray = cam.ScreenPointToRay(InputService.cursorPosition);

            if (Physics.Raycast(ray.origin,ray.direction, out hit,100f, floorLayer))
            {
                if (InDestroyMode)
                {
                    scissorsButtonTr.position = InputService.cursorPosition;
                    GetLinkToDestroy(hit.point);
                }
                else
                {
                    var point = hit.point + ray.direction * (-1 * 1f);  
                    line.positionCount++;
                    line.SetPosition(line.positionCount-1, point);
                    GetLinkable(hit.point);
                }
            }
            
            yield return null;
        }
    }
    
    #endregion
}

public class ActivateDarkmodeEvent
{
    public bool Value { get; }

    public ActivateDarkmodeEvent(bool value)
    {
        Value = value;
    }
}

public class ActivateDestroyModeEvent
{
    public bool Value { get; }

    public ActivateDestroyModeEvent(bool value)
    {
        Value = value;
    }
}

public class LinkCollisionEvent
{
    public Link Link { get; }
    public Link[] CollidingLinks { get; }

    public LinkCollisionEvent(Link link, Link[] collidingLinks)
    {
        Link = link;
        CollidingLinks = collidingLinks;
    }
}
