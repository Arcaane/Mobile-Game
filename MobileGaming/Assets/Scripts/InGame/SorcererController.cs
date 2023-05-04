using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SorcererController : MonoBehaviour
{
    [Header("Dependencies")] [SerializeField]
    private GameObject sorcererGo;
    
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
