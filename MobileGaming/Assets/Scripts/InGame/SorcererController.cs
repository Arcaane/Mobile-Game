using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SorcererController : MonoBehaviour
{
    public static SorcererController Instance;
    
    [field: SerializeField] public MagicLinesManager magicLinesManager { get; private set; }
    [field: SerializeField] public TextMeshProUGUI timeLeftText { get; private set; }
    [field: SerializeField] public TextMeshProUGUI scoreText { get; private set; }
    [field: SerializeField] public TextMeshProUGUI endGameText { get; private set; }
    [field: SerializeField] public Button endGameButton { get; private set; }
    [field: SerializeField] public GameObject endGameCanvasGo { get; private set; }

    [field: SerializeField] public GameObject hudCanvasGO;
    
    [field: SerializeField] public GameObject menuCanvasGO;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        endGameCanvasGo.SetActive(false);
    }
}
