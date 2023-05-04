using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SorcererController : MonoBehaviour
{
    
    [field: SerializeField] public MagicLinesManager magicLinesManager { get; private set; }
    [field: SerializeField] public TextMeshProUGUI timeLeftText { get; private set; }
    [field: SerializeField] public TextMeshProUGUI scoreText { get; private set; }
    [field: SerializeField] public TextMeshProUGUI endGameText { get; private set; }
    [field: SerializeField] public Button endGameButton { get; private set; }
    [field: SerializeField] public GameObject endGameCanvasGo { get; private set; }
    
    private void Start()
    {
        endGameCanvasGo.SetActive(false);
    }
    
}
