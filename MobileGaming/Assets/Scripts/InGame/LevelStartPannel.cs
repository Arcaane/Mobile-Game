using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelStartPannel : MonoBehaviour
{
    #region MyRegion
    [SerializeField] private Level myLevel;
    [SerializeField] private GameObject startLevelUI;
    [SerializeField] private TextMeshProUGUI chapterNumberText;
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI oneStarNumberText;
    [SerializeField] private TextMeshProUGUI twoStarNumberText;
    [SerializeField] private TextMeshProUGUI threeStarNumberText;

    [SerializeField] private Image[] starToActivate;
    
    private void Start()
    {
        SorcererController.Instance.hudCanvasGO.SetActive(false);
        SorcererController.Instance.menuCanvasGO.SetActive(false);
        
        Time.timeScale = 0f;
        myLevel = transform.root.GetComponent<Level>();
        
        chapterNumberText.text = $"Chapter {myLevel.currentChapter}";
        levelNumberText.text = $"Chapter {myLevel.currentLevel}";
        timerText.text = $"Chapter {myLevel.levelDuration}";
        oneStarNumberText.text = $"{myLevel.scoreToWin}";
        twoStarNumberText.text = $"{myLevel.palier2}";
        twoStarNumberText.text = $"{myLevel.palier3}";
        
        
    }
    
    #endregion
}
