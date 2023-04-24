using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MagicLinesManager : MonoBehaviour
{
    #region Variables

    // Public or Visible 
    [SerializeField] private TextMeshProUGUI debugMode;
    [SerializeField] private TextMeshProUGUI debugTimeScale;
    [SerializeField] private TextMeshProUGUI debugMana;

    [SerializeField] private LayerMask machineLayerMask;
    [SerializeField] private bool isInMagicMode;

    [SerializeField] private int maxMana;
    [SerializeField] private int currentMana;
    [SerializeField] private int bonusMana;
    public float timeToRecoverMana = 4.5f;
    
    public GameObject linePrefab;
    public Vector3[] points;
    public List<MachineLink> magicLinks;

    // Private
    private bool isPressed;
    private Ray ray;
    private RaycastHit hit;
    private Machine m1;
    private Machine m2;
    private bool isDraging => InputService.deltaPosition.x != 0 && InputService.deltaPosition.y != 0;
    private SorcererController player;

    public GameObject orthoCam;
    public GameObject perspCam;

    public GameObject currentLineInDrawning;

    private List<ILinkable> currentLinkables = new List<ILinkable>();

    #endregion
    
    private void Start()
    {
        isInMagicMode = false;
        debugMode.text = $"Magic Mode : {isInMagicMode}";
        player = GetComponent<SorcererController>();
        
        // Mana
        currentMana = maxMana + bonusMana;
        UpdateManaDebug();

        orthoCam = GameObject.Find("Ortho");
        perspCam = GameObject.Find("Persp");
        orthoCam.SetActive(false);
        
        ToggleMagic();
    }
    
    void Update()
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

    public void ToggleMagic()
    {
        isInMagicMode = !isInMagicMode;
        debugMode.text = $"Magic Mode : {isInMagicMode}";

        // Controls
        if (isInMagicMode) EnableMagicMode();
        else DisableMagicMode();
        
        // Timescale #TODO - Timescale quand t en train de tracer
        // Time.timeScale = isInMagicMode ? .6f : 1;
        // debugTimeScale.text = Time.timeScale.ToString();
    }
    
    private void EnableMagicMode()
    {
        // Inputs 
        InputService.OnPress += OnScreenTouch;
        InputService.OnRelease += OnScreenRelease;
        InputService.OnPress -= player.OnScreenTouch;
        InputService.OnRelease -= player.OnScreenRelease;
        
        // Camera
        perspCam.SetActive(false);
        orthoCam.SetActive(true);
    }

    private void DisableMagicMode()
    {
        // Inputs
        InputService.OnPress -= OnScreenTouch;
        InputService.OnRelease -= OnScreenRelease;
        InputService.OnPress += player.OnScreenTouch;
        InputService.OnRelease += player.OnScreenRelease;
        
        // Camera
        orthoCam.SetActive(false);
        perspCam.SetActive(true);

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
        
        m1 = GetClickMachine(obj);
    }

    private void OnScreenRelease(Vector2 obj)
    {
        isPressed = false;
        LinkMachines();

        if (!isDraging) return;

        m2 = GetClickMachine(obj);
        LinkMachines();
        
        currentLinkables.Clear();

        UnlinkAll();
    }

    private void UnlinkAll()
    {
        m1 = default;
        m2 = default;
    }
    
    private void LinkMachines()
    {
        // if (!currentLineInDrawning.GetComponent<DrawMagicLine>().isLinkable) return;
        
        Debug.Log($"Linking {currentLinkables.Count} machines");
        
        
        if (m2 != null && m1 != m2) return;
        
        Debug.Log($"Les machines {m1} & {m2} sont link");
        currentMana -= 1;
        CreateMagicLine();
        StartCoroutine(RecoverMana(timeToRecoverMana));
    }

    private Machine GetClickMachine(Vector2 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out hit, machineLayerMask))
        {
            var linkable = hit.transform.GetComponent<ILinkable>();
            if (linkable != null)
            {
                if(!currentLinkables.Contains(linkable)) currentLinkables.Add(linkable);
            }
            
            var col = hit.transform.GetComponent<InteractableCollider>();

            if (col == null) return null;
            
            if (col.interactable is MachineSlot slot) return slot.machine;

            return null;
        }

        return null;
    }

    #endregion

    #region Mana

    private void UpdateManaDebug()
    {
        debugMana.text = $"Mana : {currentMana}/{maxMana}";
    }
    
    IEnumerator RecoverMana(float _timeToWait)
    {
        UpdateManaDebug();
        yield return new WaitForSeconds(_timeToWait);
        currentMana++;
        UpdateManaDebug();
    }

    #endregion

    #region DrawLines&Mesh

    private Vector3 p1;
    private Vector3 p2;
    private void CreateMagicLine()
    {
        var magicLineGo = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        LineRenderer lr = magicLineGo.GetComponent<LineRenderer>();
        
        MachineLink machineLink = lr.GetComponent<MachineLink>();
        magicLinks.Add(machineLink);
        
        machineLink.machinesInLinks.Add(m1);
        m1.OnEndWork += machineLink.TakeProductFromMachine;

        machineLink.machinesInLinks.Add(m2);

        m1.outputLink = machineLink;

        p1 = m1.transform.position + (m2.transform.position - m1.transform.position).normalized * 0.7f;
        p2 = m2.transform.position + (m1.transform.position - m2.transform.position).normalized * 0.7f;
        
        Debug.DrawLine(new Vector3(p1.x, .5f, p1.z), new Vector3(p2.x, .5f, p2.z), Color.green, 20f);
        var start = new Vector3(p1.x, .5f, p1.z);
        var end = new Vector3(p2.x, .5f, p2.z);


        var hits = (Physics.RaycastAll(
            start,
            end - start,
            Vector3.Distance(start, end),
            LayerMask.NameToLayer("Link")));

        if (hits.Length > 0)
        {
            foreach (var t in hits)
            {
                var hitLink = t.transform.GetComponent<MachineLink>();
                if (hitLink != null)
                {
                    machineLink.AddDependency(hitLink);
                    machineLink.enabled = false;
                }
            }
        }
        
        if (Physics.Raycast(
                start, 
                end - start, 
                Vector3.Distance(start, end),
                LayerMask.NameToLayer("Link")))
        {
            Debug.Log("Touche un autre lien au raycast");
        }
        
        points = new[] { p1, p2 };
        lr.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            lr.SetPosition(i, points[i] + Vector3.up);
        }

        GenerateLinkCollider(lr, p1, p2);
    }
    
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

    private void FinishLine()
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
            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.y = .5f;
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, position);
            yield return null;
        }
    }
    
    #endregion
}